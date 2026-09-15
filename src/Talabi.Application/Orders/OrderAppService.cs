using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Talabi.Notifications;
using Talabi.Orders.Dtos;
using Talabi.Products;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Talabi.Orders;

[Authorize]
public class OrderAppService : ApplicationService, IOrderAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<OrderStatus, Guid> _orderStatusRepository;
    private readonly IRepository<OrderStatusHistory, Guid> _orderStatusHistoryRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly IRepository<Talabi.Payments.PaymentMethod, Guid> _paymentMethodRepository;
    private readonly INotificationSender _notificationSender;
    private readonly IRepository<CancellationReason, Guid> _cancellationReasonRepository;
    private readonly IRepository<OrderCancellation, Guid> _orderCancellationRepository;
    private readonly IRepository<OrderRejection, Guid> _orderRejectionRepository;
    private readonly IRepository<Talabi.Customers.Customer, Guid> _customerRepository;
    private readonly Talabi.Carts.IShoppingCartAppService _shoppingCartAppService;
    private readonly OrderMapper _orderMapper;

    public OrderAppService(
        IRepository<Order, Guid> orderRepository,
        IRepository<OrderStatus, Guid> orderStatusRepository,
        IRepository<OrderStatusHistory, Guid> orderStatusHistoryRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<Store, Guid> storeRepository,
        IRepository<Talabi.Payments.PaymentMethod, Guid> paymentMethodRepository,
        IRepository<CancellationReason, Guid> cancellationReasonRepository,
        IRepository<OrderCancellation, Guid> orderCancellationRepository,
        IRepository<OrderRejection, Guid> orderRejectionRepository,
        IRepository<Talabi.Customers.Customer, Guid> customerRepository,
        INotificationSender notificationSender,
        Talabi.Carts.IShoppingCartAppService shoppingCartAppService)
    {
        _orderRepository = orderRepository;
        _orderStatusRepository = orderStatusRepository;
        _orderStatusHistoryRepository = orderStatusHistoryRepository;
        _productRepository = productRepository;
        _storeRepository = storeRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _cancellationReasonRepository = cancellationReasonRepository;
        _orderCancellationRepository = orderCancellationRepository;
        _orderRejectionRepository = orderRejectionRepository;
        _customerRepository = customerRepository;
        _notificationSender = notificationSender;
        _shoppingCartAppService = shoppingCartAppService;
        _orderMapper = new OrderMapper();
    }

    public async Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListInput input)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Store, x => x.OrderStatus, x => x.Customer);

        if (input.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == input.CustomerId);
        }
        
        if (input.StoreId.HasValue)
        {
            query = query.Where(x => x.StoreId == input.StoreId);
        }

        if (input.CourierId.HasValue)
        {
            query = query.Where(x => x.CourierId == input.CourierId);
        }

        if (input.OrderStatusId.HasValue)
        {
            query = query.Where(x => x.OrderStatusId == input.OrderStatusId);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var orders = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        var orderDtos = orders.Select(o => _orderMapper.MapToOrderDto(o)).ToList();

        return new PagedResultDto<OrderDto>(totalCount, orderDtos);
    }

    public async Task<OrderDto> GetAsync(Guid id)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Store, x => x.OrderStatus, x => x.Customer, x => x.Items);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(Order), id);
        }

        var dto = _orderMapper.MapToOrderDto(order);
        if (order.Items != null)
        {
            dto.Items = order.Items.Select(x => _orderMapper.MapToOrderItemDto(x)).ToList();
        }
        return dto;
    }

    [UnitOfWork]
    public async Task<OrderDto> PlaceOrderAsync(CreateOrderInput input)
    {
        var store = await _storeRepository.GetAsync(input.StoreId);
        
        if (!store.IsActive)
        {
            throw new UserFriendlyException("عفواً، هذا المتجر غير متاح حالياً.");
        }

        var pendingStatus = await _orderStatusRepository.FirstOrDefaultAsync(x => x.Name == "Pending");
        if (pendingStatus == null)
        {
            throw new BusinessException("Order status 'Pending' is not seeded in the database.");
        }

        var paymentMethod = await _paymentMethodRepository.GetAsync(input.PaymentMethodId);
        if (paymentMethod.RequiresReceipt && !IsValidReceiptUrl(input.PaymentReceiptUrl))
        {
            throw new UserFriendlyException($"طريقة الدفع ({paymentMethod.DisplayName}) تتطلب إرفاق صورة السند/الإشعار المالي بشكل صحيح.");
        }

        // إنشاء رقم طلب عشوائي لتوضيح الفكرة (يفضل استخدام Sequence في الواقع)
        string orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        decimal subTotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var itemInput in input.Items)
        {
            var product = await _productRepository.GetAsync(itemInput.ProductId);

            if (!product.IsAvailable || !product.IsActive)
            {
                throw new UserFriendlyException($"المنتج ({product.Name}) غير متوفر حالياً.");
            }

            if (product.StoreId != input.StoreId)
            {
                throw new UserFriendlyException($"المنتج ({product.Name}) لا يتبع للمتجر المختار.");
            }

            var orderItem = new OrderItem(
                id: GuidGenerator.Create(),
                orderId: Guid.Empty, // سيتم ربطها لاحقاً
                productId: product.Id,
                productName: product.Name,
                sku: product.SKU,
                unit: product.Unit,
                quantity: itemInput.Quantity,
                unitPrice: product.Price,
                discount: product.Price - product.FinalPrice, // قيمة الخصم المالي للقطعة الواحدة إن وجد
                notes: itemInput.Notes,
                productImageUrl: product.MainImageUrl
            );

            subTotal += orderItem.TotalPrice;
            orderItems.Add(orderItem);
        }

        var currentUserId = CurrentUser.Id ?? Guid.Empty;
        var customer = await _customerRepository.FirstOrDefaultAsync(x => x.UserId == currentUserId);
        if (customer == null)
        {
            throw new UserFriendlyException("يجب إنشاء ملف شخصي للعميل أولاً قبل إتمام الطلب.");
        }

        var order = new Order(
            id: GuidGenerator.Create(),
            orderNumber: orderNumber,
            customerId: customer.Id, // استخدام المعرف الخاص بكيان العميل بدلاً من المستخدم
            storeId: input.StoreId,
            deliveryAddressId: input.DeliveryAddressId,
            deliveryAddressSnapshot: "سيتم أخذ البيانات من العنوان", // TODO: جلب بيانات العنوان الفعلية هنا
            orderStatusId: pendingStatus.Id,
            paymentMethodId: input.PaymentMethodId,
            subTotal: subTotal,
            finalAmount: subTotal, // TODO: إضافة Tax و DeliveryFee
            tenantId: CurrentTenant.Id
        );

        order.PaymentReceiptUrl = input.PaymentReceiptUrl;
        order.CustomerNotes = input.CustomerNotes;

        foreach (var item in orderItems)
        {
            item.OrderId = order.Id;
            order.Items.Add(item);
        }

        await _orderRepository.InsertAsync(order);

        var history = new OrderStatusHistory(
            id: GuidGenerator.Create(),
            orderId: order.Id,
            toStatusId: pendingStatus.Id,
            changedByUserId: CurrentUser.Id ?? Guid.Empty,
            changedByRole: "Customer",
            notes: "تم إنشاء الطلب وهو بانتظار موافقة المتجر"
        );
        await _orderStatusHistoryRepository.InsertAsync(history);

        // إرسال إشعار للمتجر (أو مالك المتجر)
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "OrderStatusChanged",
            title: "طلب جديد",
            message: $"لديك طلب جديد برقم {order.OrderNumber}",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        // إرسال إشعار تأكيد للعميل
        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderStatusChanged",
            title: "تم إرسال طلبك بنجاح",
            message: $"تم إنشاء طلبك برقم {order.OrderNumber} وهو بانتظار موافقة المتجر.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        var dto = _orderMapper.MapToOrderDto(order);
        dto.Items = order.Items.Select(x => _orderMapper.MapToOrderItemDto(x)).ToList();
        return dto;
    }

    [UnitOfWork]
    public async Task<OrderDto> CheckoutCartAsync(CheckoutCartInput input)
    {
        var cartDto = await _shoppingCartAppService.GetMyCartAsync();

        if (cartDto.Items == null || !cartDto.Items.Any())
        {
            throw new UserFriendlyException("سلة المشتريات فارغة. الرجاء إضافة منتجات قبل إتمام الطلب.");
        }

        if (cartDto.HasUnavailableItems)
        {
            throw new UserFriendlyException("تحتوي السلة على منتجات غير متاحة حالياً. الرجاء مراجعة السلة قبل المتابعة.");
        }

        var store = await _storeRepository.GetAsync(cartDto.StoreId.Value);
        
        if (!store.IsActive)
        {
            throw new UserFriendlyException("عفواً، هذا المتجر غير متاح حالياً.");
        }

        var pendingStatus = await _orderStatusRepository.FirstOrDefaultAsync(x => x.Name == "Pending");
        if (pendingStatus == null)
        {
            throw new BusinessException("Order status 'Pending' is not seeded in the database.");
        }

        var paymentMethod = await _paymentMethodRepository.GetAsync(input.PaymentMethodId);
        if (paymentMethod.RequiresReceipt && !IsValidReceiptUrl(input.PaymentReceiptUrl))
        {
            throw new UserFriendlyException($"طريقة الدفع ({paymentMethod.DisplayName}) تتطلب إرفاق صورة السند/الإشعار المالي بشكل صحيح.");
        }

        string orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        var order = new Order(
            id: GuidGenerator.Create(),
            orderNumber: orderNumber,
            customerId: cartDto.CustomerId,
            storeId: cartDto.StoreId.Value,
            deliveryAddressId: input.DeliveryAddressId,
            deliveryAddressSnapshot: "سيتم أخذ البيانات من العنوان", // TODO: جلب بيانات العنوان الفعلية هنا
            orderStatusId: pendingStatus.Id,
            paymentMethodId: input.PaymentMethodId,
            subTotal: cartDto.SubTotal,
            finalAmount: cartDto.FinalTotal,
            tenantId: CurrentTenant.Id
        );

        order.PaymentReceiptUrl = input.PaymentReceiptUrl;
        order.CustomerNotes = input.CustomerNotes;

        foreach (var itemDto in cartDto.Items)
        {
            var product = await _productRepository.GetAsync(itemDto.ProductId);
            var orderItem = new OrderItem(
                id: GuidGenerator.Create(),
                orderId: order.Id,
                productId: itemDto.ProductId,
                productName: itemDto.ProductName,
                sku: product.SKU,
                unit: product.Unit,
                quantity: itemDto.Quantity,
                unitPrice: itemDto.OriginalUnitPrice,
                discount: itemDto.UnitDiscount,
                notes: itemDto.Notes,
                productImageUrl: itemDto.ProductImageUrl
            );
            order.Items.Add(orderItem);
        }

        await _orderRepository.InsertAsync(order);

        var history = new OrderStatusHistory(
            id: GuidGenerator.Create(),
            orderId: order.Id,
            toStatusId: pendingStatus.Id,
            changedByUserId: CurrentUser.Id ?? Guid.Empty,
            changedByRole: "Customer",
            notes: "تم إنشاء الطلب وهو بانتظار موافقة المتجر"
        );
        await _orderStatusHistoryRepository.InsertAsync(history);

        // إرسال إشعار للمتجر
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "OrderStatusChanged",
            title: "طلب جديد",
            message: $"لديك طلب جديد برقم {order.OrderNumber}",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        // إرسال إشعار تأكيد للعميل
        var customer = await _customerRepository.GetAsync(cartDto.CustomerId);
        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderStatusChanged",
            title: "تم إرسال طلبك بنجاح",
            message: $"تم إنشاء طلبك برقم {order.OrderNumber} وهو بانتظار موافقة المتجر.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        // تفريغ السلة بعد نجاح إنشاء الطلب
        await _shoppingCartAppService.ClearCartAsync();

        var dto = _orderMapper.MapToOrderDto(order);
        dto.Items = order.Items.Select(x => _orderMapper.MapToOrderItemDto(x)).ToList();
        return dto;
    }


    [UnitOfWork]
    public async Task<OrderDto> AttachPaymentReceiptAsync(Guid id, string receiptUrl)
    {
        var order = await _orderRepository.GetAsync(id);
        order.PaymentReceiptUrl = receiptUrl;
        await _orderRepository.UpdateAsync(order);
        return await GetAsync(id);
    }

    [UnitOfWork]
    public async Task<OrderDto> ChangeOrderStatusAsync(Guid id, Guid newStatusId, string? notes = null)
    {
        var order = await _orderRepository.GetAsync(id);
        var oldStatusId = order.OrderStatusId;
        var newStatus = await _orderStatusRepository.GetAsync(newStatusId);

        order.OrderStatusId = newStatusId;
        
        var history = new OrderStatusHistory(
            id: GuidGenerator.Create(),
            orderId: order.Id,
            toStatusId: newStatusId,
            changedByUserId: CurrentUser.Id ?? Guid.Empty,
            changedByRole: "System",
            fromStatusId: oldStatusId,
            notes: notes ?? $"تم تحديث حالة الطلب إلى: {newStatus.DisplayName}"
        );
        await _orderStatusHistoryRepository.InsertAsync(history);

        await _orderRepository.UpdateAsync(order);

        var customer = await _customerRepository.GetAsync(order.CustomerId);

        // إشعار العميل بتحديث الطلب
        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderStatusChanged",
            title: "تحديث حالة الطلب",
            message: $"تحديث على طلبك #{order.OrderNumber}: {newStatus.DisplayName}",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        return await GetAsync(id);
    }

    [UnitOfWork]
    public async Task<OrderDto> AssignDriverAsync(Guid id, Guid courierId)
    {
        var order = await _orderRepository.GetAsync(id);
        order.CourierId = courierId;

        await _orderRepository.UpdateAsync(order);

        // إشعار السائق بأنه تم تكليفه
        await _notificationSender.SendToUserAsync(
            recipientUserId: courierId,
            notificationTypeName: "OrderAssignedToDriver",
            title: "تكليف بطلب جديد",
            message: $"تم تكليفك بتوصيل الطلب #{order.OrderNumber}. يرجى التوجه للمتجر.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        return await GetAsync(id);
    }

    [UnitOfWork]
    public async Task<OrderDto> CancelOrderAsync(Guid id, CancelOrderInput input)
    {
        var order = await _orderRepository.GetAsync(id);
        var oldStatusId = order.OrderStatusId;

        // Verify it's in a state that can be cancelled (e.g., Pending)
        var currentStatus = await _orderStatusRepository.GetAsync(oldStatusId);
        if (currentStatus.Name != "Pending")
        {
            throw new UserFriendlyException("لا يمكن إلغاء الطلب في هذه المرحلة.");
        }

        var cancelledStatus = await _orderStatusRepository.FirstOrDefaultAsync(x => x.Name == "Cancelled");
        if (cancelledStatus == null) throw new BusinessException("Status 'Cancelled' not found.");

        var reasonId = input.CancellationReasonId;
        if (!reasonId.HasValue || reasonId.Value == Guid.Empty)
        {
            var defaultReason = await _cancellationReasonRepository.FirstOrDefaultAsync(x => x.TargetAudience == CancellationTargetAudience.Customer);
            if (defaultReason == null)
            {
                defaultReason = await _cancellationReasonRepository.InsertAsync(
                    new CancellationReason(GuidGenerator.Create(), "أخرى", CancellationTargetAudience.Customer),
                    autoSave: true
                );
            }
            reasonId = defaultReason.Id;
        }

        // Record cancellation
        var cancellation = new OrderCancellation(
            GuidGenerator.Create(),
            order.Id,
            CurrentUser.Id ?? Guid.Empty,
            reasonId.Value,
            "Customer", // Assuming customer cancels it
            oldStatusId,
            input.AdditionalNotes
        );
        await _orderCancellationRepository.InsertAsync(cancellation);

        order.OrderStatusId = cancelledStatus.Id;
        order.CancelledAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        // Record history
        var history = new OrderStatusHistory(
            GuidGenerator.Create(),
            order.Id,
            cancelledStatus.Id,
            CurrentUser.Id ?? Guid.Empty,
            "Customer",
            oldStatusId,
            "تم إلغاء الطلب من قبل العميل"
        );
        await _orderStatusHistoryRepository.InsertAsync(history);

        var store = await _storeRepository.GetAsync(order.StoreId);
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "OrderCancelled",
            title: "إلغاء طلب",
            message: $"قام العميل بإلغاء الطلب #{order.OrderNumber}.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        return await GetAsync(id);
    }

    [UnitOfWork]
    public async Task<OrderDto> AcceptOrderAsync(Guid id, string? notes = null)
    {
        var order = await _orderRepository.GetAsync(id);
        var oldStatusId = order.OrderStatusId;

        var acceptedStatus = await _orderStatusRepository.FirstOrDefaultAsync(x => x.Name == "Accepted" || x.Name == "Preparing");
        if (acceptedStatus == null) throw new BusinessException("Target status not found.");

        order.OrderStatusId = acceptedStatus.Id;
        order.ConfirmedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        var history = new OrderStatusHistory(
            GuidGenerator.Create(),
            order.Id,
            acceptedStatus.Id,
            CurrentUser.Id ?? Guid.Empty,
            "Store",
            oldStatusId,
            notes ?? "تم قبول الطلب من قبل المتجر"
        );
        await _orderStatusHistoryRepository.InsertAsync(history);

        var customer = await _customerRepository.GetAsync(order.CustomerId);

        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderAccepted",
            title: "تم قبول طلبك",
            message: $"قام المتجر بقبول طلبك #{order.OrderNumber}.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        return await GetAsync(id);
    }

    [UnitOfWork]
    public async Task<OrderDto> RejectOrderAsync(Guid id, RejectOrderInput input)
    {
        var order = await _orderRepository.GetAsync(id);
        var oldStatusId = order.OrderStatusId;

        var rejectedStatus = await _orderStatusRepository.FirstOrDefaultAsync(x => x.Name == "Rejected");
        if (rejectedStatus == null) throw new BusinessException("Status 'Rejected' not found.");

        var reasonId = input.RejectionReasonId;
        if (!reasonId.HasValue || reasonId.Value == Guid.Empty)
        {
            var defaultReason = await _cancellationReasonRepository.FirstOrDefaultAsync(x => x.TargetAudience == CancellationTargetAudience.Store);
            if (defaultReason == null)
            {
                defaultReason = await _cancellationReasonRepository.InsertAsync(
                    new CancellationReason(GuidGenerator.Create(), "أخرى", CancellationTargetAudience.Store),
                    autoSave: true
                );
            }
            reasonId = defaultReason.Id;
        }

        var rejection = new OrderRejection(
            GuidGenerator.Create(),
            order.Id,
            CurrentUser.Id ?? Guid.Empty,
            reasonId.Value,
            input.AdditionalNotes
        );
        await _orderRejectionRepository.InsertAsync(rejection);

        order.OrderStatusId = rejectedStatus.Id;
        await _orderRepository.UpdateAsync(order);

        var history = new OrderStatusHistory(
            GuidGenerator.Create(),
            order.Id,
            rejectedStatus.Id,
            CurrentUser.Id ?? Guid.Empty,
            "Store",
            oldStatusId,
            "تم رفض الطلب من قبل المتجر"
        );
        await _orderStatusHistoryRepository.InsertAsync(history);

        var customer = await _customerRepository.GetAsync(order.CustomerId);

        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderRejected",
            title: "نعتذر، تم رفض طلبك",
            message: $"اعتذر المتجر عن تلبية طلبك #{order.OrderNumber}.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        return await GetAsync(id);
    }

    [UnitOfWork]
    public async Task<OrderDto> CompleteOrderAsync(Guid id, string? notes = null)
    {
        var order = await _orderRepository.GetAsync(id);
        var oldStatusId = order.OrderStatusId;

        var deliveredStatus = await _orderStatusRepository.FirstOrDefaultAsync(x => x.Name == "Delivered");
        if (deliveredStatus == null) throw new BusinessException("Status 'Delivered' not found.");

        order.OrderStatusId = deliveredStatus.Id;
        order.ActualDeliveryTime = DateTime.UtcNow;
        if (order.PaymentStatus == OrderPaymentStatus.Pending)
        {
            order.PaymentStatus = OrderPaymentStatus.Paid; // Usually marked paid when delivered if COD
        }
        await _orderRepository.UpdateAsync(order);

        var history = new OrderStatusHistory(
            GuidGenerator.Create(),
            order.Id,
            deliveredStatus.Id,
            CurrentUser.Id ?? Guid.Empty,
            "System",
            oldStatusId,
            notes ?? "تم توصيل الطلب بنجاح"
        );
        await _orderStatusHistoryRepository.InsertAsync(history);

        var customer = await _customerRepository.GetAsync(order.CustomerId);

        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderDelivered",
            title: "تم التسليم",
            message: $"تم تسليم طلبك #{order.OrderNumber} بنجاح. نتمنى أن ينال إعجابك!",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        return await GetAsync(id);
    }

    public async Task<List<CancellationReasonDto>> GetCancellationReasonsAsync(bool activeOnly = true)
    {
        var query = await _cancellationReasonRepository.GetQueryableAsync();
        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        var reasons = await AsyncExecuter.ToListAsync(query);
        return reasons.Select(r => new CancellationReasonDto
        {
            Id = r.Id,
            Reason = r.Reason,
            TargetAudience = r.TargetAudience,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<List<OrderStatusDto>> GetOrderStatusesAsync()
    {
        var statuses = await _orderStatusRepository.GetListAsync();
        return statuses.OrderBy(x => x.DisplayOrder).Select(s => new OrderStatusDto
        {
            Id = s.Id,
            Name = s.Name,
            DisplayName = s.DisplayName,
            DisplayOrder = s.DisplayOrder,
            Color = s.Color,
            Icon = s.Icon
        }).ToList();
    }

    /// <summary>
    /// التحقق من صحة رابط إيصال الدفع وأنه ليس فارغاً أو قيمة افتراضية مثل "string"
    /// </summary>
    private static bool IsValidReceiptUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        var trimmed = url.Trim();
        if (trimmed.Equals("string", StringComparison.OrdinalIgnoreCase)) return false;
        return trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("/", StringComparison.OrdinalIgnoreCase);
    }
}
