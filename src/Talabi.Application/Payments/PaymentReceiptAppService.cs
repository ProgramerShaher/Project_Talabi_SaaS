using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Customers;
using Talabi.MediaFiles;
using Talabi.Notifications;
using Talabi.Orders;
using Talabi.Payments.Dtos;
using Talabi.Permissions;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Talabi.Payments;

/// <summary>
/// خدمة مراجعة والتحقق من إيصالات الدفع والتحويلات المالية
/// </summary>
[Authorize]
public class PaymentReceiptAppService : ApplicationService, IPaymentReceiptAppService
{
    private readonly IRepository<PaymentReceipt, Guid> _paymentReceiptRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<PaymentMethod, Guid> _paymentMethodRepository;
    private readonly IRepository<MediaFile, Guid> _mediaFileRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly INotificationSender _notificationSender;

    public PaymentReceiptAppService(
        IRepository<PaymentReceipt, Guid> paymentReceiptRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Order, Guid> orderRepository,
        IRepository<Store, Guid> storeRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<PaymentMethod, Guid> paymentMethodRepository,
        IRepository<MediaFile, Guid> mediaFileRepository,
        IRepository<IdentityUser, Guid> userRepository,
        INotificationSender notificationSender)
    {
        _paymentReceiptRepository = paymentReceiptRepository;
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _storeRepository = storeRepository;
        _customerRepository = customerRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _mediaFileRepository = mediaFileRepository;
        _userRepository = userRepository;
        _notificationSender = notificationSender;
    }

    /// <summary>
    /// الحصول على تفاصيل إيصال دفع بواسطة معرفه الفريد
    /// </summary>
    public async Task<PaymentReceiptDetailsDto> GetAsync(Guid id)
    {
        var receipt = await _paymentReceiptRepository.GetAsync(id);
        var payment = await _paymentRepository.GetAsync(receipt.PaymentId);
        var order = await _orderRepository.GetAsync(payment.OrderId);

        await ValidateAccessToReceiptAsync(order);

        return await BuildReceiptDetailsDtoAsync(receipt, payment, order);
    }

    /// <summary>
    /// الحصول على تفاصيل إيصال الدفع لطلب محدد
    /// </summary>
    public async Task<PaymentReceiptDetailsDto?> GetByOrderIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId);
        await ValidateAccessToReceiptAsync(order);

        var payment = await _paymentRepository.FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (payment == null)
        {
            return null;
        }

        var receipt = await _paymentReceiptRepository.FirstOrDefaultAsync(x => x.PaymentId == payment.Id);
        if (receipt == null)
        {
            return null;
        }

        return await BuildReceiptDetailsDtoAsync(receipt, payment, order);
    }

    /// <summary>
    /// الحصول على تفاصيل إيصال الدفع لعملية دفع محددة
    /// </summary>
    public async Task<PaymentReceiptDetailsDto?> GetByPaymentIdAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetAsync(paymentId);
        var order = await _orderRepository.GetAsync(payment.OrderId);

        await ValidateAccessToReceiptAsync(order);

        var receipt = await _paymentReceiptRepository.FirstOrDefaultAsync(x => x.PaymentId == paymentId);
        if (receipt == null)
        {
            return null;
        }

        return await BuildReceiptDetailsDtoAsync(receipt, payment, order);
    }

    /// <summary>
    /// استعراض وتصفية قائمة إيصالات الدفع لأصحاب المتاجر وإدارة النظام
    /// </summary>
    public async Task<PagedResultDto<PaymentReceiptDetailsDto>> GetListAsync(GetPaymentReceiptListInput input)
    {
        var currentUserId = CurrentUser.GetId();
        var hasManagePermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Payments.Manage);
        var hasVerifyPermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Payments.VerifyReceipts);

        // جلب متاجر المستخدم للتحقق من العزل الأمني
        var userStores = await _storeRepository.GetListAsync(x => x.OwnerId == currentUserId);
        var userStoreIds = userStores.Select(x => x.Id).ToList();

        if (!hasManagePermission && !userStoreIds.Any() && !hasVerifyPermission)
        {
            return new PagedResultDto<PaymentReceiptDetailsDto>(0, new List<PaymentReceiptDetailsDto>());
        }

        if (input.StoreId.HasValue && !hasManagePermission)
        {
            if (!userStoreIds.Contains(input.StoreId.Value))
            {
                throw new UserFriendlyException("غير مصرح لك بالاطلاع على إيصالات هذا المتجر.");
            }
        }

        var receiptQueryable = await _paymentReceiptRepository.GetQueryableAsync();
        var paymentQueryable = await _paymentRepository.GetQueryableAsync();
        var orderQueryable = await _orderRepository.GetQueryableAsync();

        var query = from r in receiptQueryable
                    join p in paymentQueryable on r.PaymentId equals p.Id
                    join o in orderQueryable on p.OrderId equals o.Id
                    select new { Receipt = r, Payment = p, Order = o };

        // تصفية الصلاحيات للمتجر في حال لم يكن مديراً عاماً
        if (!hasManagePermission)
        {
            if (input.StoreId.HasValue)
            {
                query = query.Where(x => x.Order.StoreId == input.StoreId.Value);
            }
            else
            {
                query = query.Where(x => userStoreIds.Contains(x.Order.StoreId));
            }
        }
        else if (input.StoreId.HasValue)
        {
            query = query.Where(x => x.Order.StoreId == input.StoreId.Value);
        }

        // تصفية حسب الطلب
        if (input.OrderId.HasValue)
        {
            query = query.Where(x => x.Order.Id == input.OrderId.Value);
        }

        // تصفية حسب حالة التحقق
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Receipt.VerificationStatus == input.Status.Value);
        }

        // تصفية حسب التاريخ
        if (input.StartDate.HasValue)
        {
            query = query.Where(x => x.Receipt.UploadedAt >= input.StartDate.Value);
        }

        if (input.EndDate.HasValue)
        {
            query = query.Where(x => x.Receipt.UploadedAt <= input.EndDate.Value);
        }

        // تصفية بالبحث النصي
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim().ToLower();
            query = query.Where(x =>
                x.Order.OrderNumber.ToLower().Contains(filter) ||
                (x.Receipt.TransactionNumber != null && x.Receipt.TransactionNumber.ToLower().Contains(filter)) ||
                (x.Receipt.WalletName != null && x.Receipt.WalletName.ToLower().Contains(filter)));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        // الترتيب الافتراضي من الأحدث إلى الأقدم
        query = query.OrderByDescending(x => x.Receipt.UploadedAt)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount);

        var pagedItems = await AsyncExecuter.ToListAsync(query);

        var resultDtos = new List<PaymentReceiptDetailsDto>();
        foreach (var item in pagedItems)
        {
            var dto = await BuildReceiptDetailsDtoAsync(item.Receipt, item.Payment, item.Order);
            resultDtos.Add(dto);
        }

        return new PagedResultDto<PaymentReceiptDetailsDto>(totalCount, resultDtos);
    }

    /// <summary>
    /// تقديم إيصال دفع جديد أو إعادة رفع إيصال بعد الرفض
    /// </summary>
    public async Task<PaymentReceiptDetailsDto> SubmitReceiptAsync(SubmitPaymentReceiptInput input)
    {
        if (!input.PaymentId.HasValue && !input.OrderId.HasValue)
        {
            throw new UserFriendlyException("يرجى تزويد معرف الطلب أو معرف عملية الدفع.");
        }

        Order order;
        Payment? payment = null;

        if (input.PaymentId.HasValue)
        {
            payment = await _paymentRepository.GetAsync(input.PaymentId.Value);
            order = await _orderRepository.GetAsync(payment.OrderId);
        }
        else
        {
            order = await _orderRepository.GetAsync(input.OrderId!.Value);
            payment = await _paymentRepository.FirstOrDefaultAsync(x => x.OrderId == order.Id);
        }

        // التحقق من صلاحية العميل صاحب الطلب
        await ValidateCustomerOrderOwnershipAsync(order);

        // التأكد من وجود كيان عملية الدفع
        if (payment == null)
        {
            payment = new Payment(
                id: GuidGenerator.Create(),
                orderId: order.Id,
                paymentMethodId: order.PaymentMethodId,
                amount: order.FinalAmount
            );
            await _paymentRepository.InsertAsync(payment);
        }

        // الحصول على رابط الملف العام
        var mediaFile = await _mediaFileRepository.FindAsync(input.MediaFileId);
        if (mediaFile != null && !string.IsNullOrWhiteSpace(mediaFile.PublicUrl))
        {
            order.PaymentReceiptUrl = mediaFile.PublicUrl;
            await _orderRepository.UpdateAsync(order);
        }

        // التحقق مما إذا كان هناك إيصال مسجل سابقاً لنفس عملية الدفع
        var receipt = await _paymentReceiptRepository.FirstOrDefaultAsync(x => x.PaymentId == payment.Id);
        if (receipt == null)
        {
            receipt = new PaymentReceipt(
                id: GuidGenerator.Create(),
                paymentId: payment.Id,
                mediaFileId: input.MediaFileId,
                walletName: input.WalletName,
                transactionNumber: input.TransactionNumber,
                amount: input.Amount ?? order.FinalAmount
            );
            await _paymentReceiptRepository.InsertAsync(receipt);
        }
        else
        {
            // إعادة تقديم الإيصال وتصفير حالة الرفض السابقة
            receipt.Resubmit(
                mediaFileId: input.MediaFileId,
                walletName: input.WalletName,
                transactionNumber: input.TransactionNumber,
                amount: input.Amount ?? order.FinalAmount
            );
            await _paymentReceiptRepository.UpdateAsync(receipt);
        }

        // إعادة ضبط حالة الدفع للطلب والمعاملة إلى قيد الانتظار
        payment.Status = PaymentTransactionStatus.Pending;
        order.PaymentStatus = OrderPaymentStatus.Pending;
        await _paymentRepository.UpdateAsync(payment);
        await _orderRepository.UpdateAsync(order);

        // إرسال إشعار للمتجر بوجود إيصال بانتظار المراجعة
        var store = await _storeRepository.GetAsync(order.StoreId);
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "PaymentReceiptSubmitted",
            title: "إيصال دفع جديد",
            message: $"تم رفع إيصال دفع جديد للطلب رقم {order.OrderNumber} وهو بانتظار المراجعة والتحقق.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        Logger.LogInformation(
            "Payment receipt {ReceiptId} submitted for Order {OrderNumber} ({OrderId}) with MediaFile {MediaFileId}",
            receipt.Id, order.OrderNumber, order.Id, input.MediaFileId);

        return await BuildReceiptDetailsDtoAsync(receipt, payment, order);
    }

    /// <summary>
    /// مراجعة والتحقق من إيصال الدفع (قبول أو رفض مع تدوين السبب)
    /// </summary>
    public async Task<PaymentReceiptDetailsDto> VerifyReceiptAsync(VerifyPaymentReceiptInput input)
    {
        var receipt = await _paymentReceiptRepository.GetAsync(input.ReceiptId);
        var payment = await _paymentRepository.GetAsync(receipt.PaymentId);
        var order = await _orderRepository.GetAsync(payment.OrderId);
        var store = await _storeRepository.GetAsync(order.StoreId);

        var currentUserId = CurrentUser.GetId();
        var isOwner = store.OwnerId == currentUserId;
        var hasVerifyPermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Payments.VerifyReceipts);
        var hasManagePermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Payments.Manage);

        if (!isOwner && !hasVerifyPermission && !hasManagePermission)
        {
            throw new UserFriendlyException("غير مصرح لك بالتحقق من هذا الإيصال، الإجراء متاح لمالك المتجر أو إدارة النظام فقط.");
        }

        if (!input.IsApproved && string.IsNullOrWhiteSpace(input.RejectionReason))
        {
            throw new UserFriendlyException("يرجى تحديد سبب رفض إيصال الدفع لتوضيحه للعميل.");
        }

        var customer = await _customerRepository.FindAsync(order.CustomerId);
        if (customer != null && customer.UserId == currentUserId && !isOwner)
        {
            throw new UserFriendlyException("لا يمكن للعميل مراجعة أو اعتماد إيصال الدفع الخاص به.");
        }

        if (input.IsApproved)
        {
            receipt.Verify(currentUserId);
            payment.MarkAsCompleted();
            order.PaymentStatus = OrderPaymentStatus.Paid;

            if (customer != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: customer.UserId,
                    notificationTypeName: "PaymentReceiptVerified",
                    title: "تم قبول إيصال الدفع",
                    message: $"تم اعتماد إيصال الدفع لطلبك رقم {order.OrderNumber} وتأكيد عملية السداد بنجاح.",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id
                );
            }

            Logger.LogInformation(
                "Receipt {ReceiptId} for Order {OrderNumber} approved by User {UserId}",
                receipt.Id, order.OrderNumber, currentUserId);
        }
        else
        {
            receipt.Reject(currentUserId, input.RejectionReason!.Trim());
            payment.MarkAsFailed();
            order.PaymentStatus = OrderPaymentStatus.Pending;

            if (customer != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: customer.UserId,
                    notificationTypeName: "PaymentReceiptRejected",
                    title: "تم رفض إيصال الدفع",
                    message: $"تم رفض إيصال الدفع لطلبك رقم {order.OrderNumber}. السبب: {input.RejectionReason}",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id
                );
            }

            Logger.LogInformation(
                "Receipt {ReceiptId} for Order {OrderNumber} rejected by User {UserId}. Reason: {Reason}",
                receipt.Id, order.OrderNumber, currentUserId, input.RejectionReason);
        }

        await _paymentReceiptRepository.UpdateAsync(receipt);
        await _paymentRepository.UpdateAsync(payment);
        await _orderRepository.UpdateAsync(order);

        return await BuildReceiptDetailsDtoAsync(receipt, payment, order);
    }

    #region Helper Methods

    /// <summary>
    /// التحقق من أن المستخدم الحالي لديه صلاحية الوصول للإيصال (عميل صاحب الطلب، أو مالك المتجر، أو إدارة النظام)
    /// </summary>
    private async Task ValidateAccessToReceiptAsync(Order order)
    {
        var currentUserId = CurrentUser.GetId();
        var hasManagePermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Payments.Manage);
        var hasVerifyPermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Payments.VerifyReceipts);

        if (hasManagePermission || hasVerifyPermission)
        {
            return;
        }

        var store = await _storeRepository.FindAsync(order.StoreId);
        if (store != null && store.OwnerId == currentUserId)
        {
            return;
        }

        var customer = await _customerRepository.FindAsync(order.CustomerId);
        if (customer != null && customer.UserId == currentUserId)
        {
            return;
        }

        throw new UserFriendlyException("غير مصرح لك بالاطلاع على تفاصيل هذا الإيصال.");
    }

    /// <summary>
    /// التحقق من ملكية العميل للطلب في حال كان المستخدم هو العميل
    /// </summary>
    private async Task ValidateCustomerOrderOwnershipAsync(Order order)
    {
        var currentUserId = CurrentUser.GetId();
        var hasManagePermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Payments.Manage);
        var hasVerifyPermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Payments.VerifyReceipts);

        if (hasManagePermission || hasVerifyPermission)
        {
            return;
        }

        var store = await _storeRepository.FindAsync(order.StoreId);
        if (store != null && store.OwnerId == currentUserId)
        {
            return;
        }

        var customer = await _customerRepository.FindAsync(order.CustomerId);
        if (customer != null && customer.UserId == currentUserId)
        {
            return;
        }

        throw new UserFriendlyException("عفواً، لا يمكنك رفع إيصال دفع لطلب لا يخصك.");
    }

    /// <summary>
    /// بناء وتجهيز كائن العرض التفصيلي لإيصال الدفع
    /// </summary>
    private async Task<PaymentReceiptDetailsDto> BuildReceiptDetailsDtoAsync(
        PaymentReceipt receipt,
        Payment payment,
        Order order)
    {
        var dto = new PaymentReceiptDetailsDto
        {
            Id = receipt.Id,
            PaymentId = receipt.PaymentId,
            MediaFileId = receipt.MediaFileId,
            WalletName = receipt.WalletName,
            TransactionNumber = receipt.TransactionNumber,
            Amount = receipt.Amount,
            VerificationStatus = receipt.VerificationStatus,
            VerifiedByUserId = receipt.VerifiedByUserId,
            VerifiedAt = receipt.VerifiedAt,
            RejectionReason = receipt.RejectionReason,
            UploadedAt = receipt.UploadedAt,
            CreationTime = receipt.CreationTime,
            CreatorId = receipt.CreatorId,
            LastModificationTime = receipt.LastModificationTime,
            LastModifierId = receipt.LastModifierId,
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderFinalAmount = order.FinalAmount,
            StoreId = order.StoreId
        };

        // رابط صورة الإيصال
        if (receipt.MediaFileId != Guid.Empty)
        {
            var mediaFile = await _mediaFileRepository.FindAsync(receipt.MediaFileId);
            dto.ReceiptFileUrl = mediaFile?.PublicUrl ?? order.PaymentReceiptUrl;
        }
        else
        {
            dto.ReceiptFileUrl = order.PaymentReceiptUrl;
        }

        // بيانات المتجر
        var store = await _storeRepository.FindAsync(order.StoreId);
        if (store != null)
        {
            dto.StoreName = store.Name;
        }

        // بيانات العميل
        var customer = await _customerRepository.FindAsync(order.CustomerId);
        if (customer != null)
        {
            dto.CustomerId = customer.Id;
            var customerUser = await _userRepository.FindAsync(customer.UserId);
            if (customerUser != null)
            {
                var fullName = $"{customerUser.Name} {customerUser.Surname}".Trim();
                dto.CustomerName = !string.IsNullOrWhiteSpace(fullName) ? fullName : customerUser.UserName;
                dto.CustomerPhoneNumber = customerUser.PhoneNumber;
            }
            else
            {
                dto.CustomerName = "عميل طلبي";
            }
        }

        // طريقة الدفع
        var paymentMethod = await _paymentMethodRepository.FindAsync(payment.PaymentMethodId);
        if (paymentMethod != null)
        {
            dto.PaymentMethodDisplayName = paymentMethod.DisplayName;
        }

        // اسم المستخدم الذي دقق الإيصال
        if (receipt.VerifiedByUserId.HasValue)
        {
            var user = await _userRepository.FindAsync(receipt.VerifiedByUserId.Value);
            dto.VerifiedByUserName = user?.UserName;
        }

        return dto;
    }

    #endregion
}
