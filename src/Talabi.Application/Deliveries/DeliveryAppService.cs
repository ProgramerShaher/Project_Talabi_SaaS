using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Customers;
using Talabi.Deliveries.Dtos;
using Talabi.Notifications;
using Talabi.Orders;
using Talabi.Permissions;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace Talabi.Deliveries;

/// <summary>
/// خدمة التطبيق لإدارة دورة مشاوير التوصيل وتتبعها وتأكيد تسليم الطلبات
/// </summary>
[Authorize]
public class DeliveryAppService : ApplicationService, IDeliveryAppService
{
    #region Fields & Dependencies
    private readonly IRepository<DeliveryAssignment, Guid> _assignmentRepository;
    private readonly IRepository<DeliveryConfirmation, Guid> _confirmationRepository;
    private readonly IRepository<Courier, Guid> _courierRepository;
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<OrderStatus, Guid> _orderStatusRepository;
    private readonly IRepository<OrderStatusHistory, Guid> _orderStatusHistoryRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly INotificationSender _notificationSender;
    private readonly DeliveryMapper _mapper;
    #endregion

    #region Constructors
    /// <summary>
    /// منشئ خدمة التوصيل وحقن الاعتماديات المطلوبة
    /// </summary>
    public DeliveryAppService(
        IRepository<DeliveryAssignment, Guid> assignmentRepository,
        IRepository<DeliveryConfirmation, Guid> confirmationRepository,
        IRepository<Courier, Guid> courierRepository,
        IRepository<Order, Guid> orderRepository,
        IRepository<OrderStatus, Guid> orderStatusRepository,
        IRepository<OrderStatusHistory, Guid> orderStatusHistoryRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Store, Guid> storeRepository,
        IIdentityUserRepository identityUserRepository,
        INotificationSender notificationSender)
    {
        _assignmentRepository = assignmentRepository;
        _confirmationRepository = confirmationRepository;
        _courierRepository = courierRepository;
        _orderRepository = orderRepository;
        _orderStatusRepository = orderStatusRepository;
        _orderStatusHistoryRepository = orderStatusHistoryRepository;
        _customerRepository = customerRepository;
        _storeRepository = storeRepository;
        _identityUserRepository = identityUserRepository;
        _notificationSender = notificationSender;
        _mapper = new DeliveryMapper();
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// إسناد وتعيين طلب لمندوب توصيل محدد
    /// </summary>
    [Authorize(TalabiPermissions.Deliveries.Assign)]
    [UnitOfWork]
    public async Task<DeliveryAssignmentDto> AssignCourierAsync(AssignCourierInput input)
    {
        var order = await _orderRepository.GetAsync(input.OrderId);
        var courier = await _courierRepository.GetAsync(input.CourierId);

        if (!courier.IsAvailable)
        {
            throw new UserFriendlyException("المندوب المحدد غير متاح حالياً لاستلام طلبات جديدة.");
        }

        // فحص هل يوجد تعيين سابق لهذا الطلب
        var assignment = await _assignmentRepository.FirstOrDefaultAsync(x => x.OrderId == input.OrderId);

        if (assignment == null)
        {
            assignment = new DeliveryAssignment(
                GuidGenerator.Create(),
                input.OrderId,
                input.CourierId,
                CurrentUser.GetId()
            );
            await _assignmentRepository.InsertAsync(assignment, autoSave: true);
        }
        else
        {
            assignment.CourierId = input.CourierId;
            assignment.AssignedByUserId = CurrentUser.GetId();
            assignment.Status = DeliveryAssignmentStatus.Assigned;
            assignment.AssignedAt = Clock.Now;
            assignment.AcceptedAt = null;
            assignment.PickupAt = null;
            assignment.DeliveredAt = null;

            await _assignmentRepository.UpdateAsync(assignment, autoSave: true);
        }

        // تحديث معرف المندوب في الطلب
        order.CourierId = courier.Id;
        await _orderRepository.UpdateAsync(order, autoSave: true);

        // جعل المندوب مشغولاً بهذا الطلب
        courier.IsAvailable = false;
        await _courierRepository.UpdateAsync(courier, autoSave: true);

        var store = await _storeRepository.FindAsync(order.StoreId);

        // إرسال إشعار للمندوب
        await _notificationSender.SendToUserAsync(
            recipientUserId: courier.UserId,
            notificationTypeName: "OrderAssignedToCourier",
            title: "طلب توصيل جديد",
            message: $"تم تكليفك بتوصيل الطلب رقم #{order.OrderNumber} من متجر \"{store?.Name ?? "المتجر"}\". يرجى التوجه للتطبيق لقبول المشوار.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: "/courier/deliveries"
        );

        if (store != null)
        {
            // إشعار المتجر بتعيين المندوب
            await _notificationSender.SendToUserAsync(
                recipientUserId: store.OwnerId,
                notificationTypeName: "OrderStatusChanged",
                title: "تم تعيين مندوب للطلب",
                message: $"تم إسناد الطلب #{order.OrderNumber} للمندوب، وبانتظار قبوله للمشوار والتوجه لاستلامه.",
                relatedEntityName: "Order",
                relatedEntityId: order.Id,
                actionUrl: $"/store/orders/{order.Id}"
            );
        }

        Logger.LogInformation(
            "تم إسناد الطلب {OrderId} للمندوب {CourierId} بنجاح عبر التعيين: {AssignmentId}",
            order.Id, courier.Id, assignment.Id);

        return await MapToDtoWithDetailsAsync(assignment);
    }

    /// <summary>
    /// جلب المشوار والتوصيلة النشطة حالياً للمندوب المسجل دخوله
    /// </summary>
    public async Task<DeliveryAssignmentDto?> GetMyActiveDeliveryAsync()
    {
        var courier = await GetCurrentCourierOrNullAsync();
        if (courier == null)
        {
            return null;
        }

        var activeAssignment = await AsyncExecuter.FirstOrDefaultAsync(
            (await _assignmentRepository.GetQueryableAsync())
                .Where(x => x.CourierId == courier.Id &&
                            x.Status != DeliveryAssignmentStatus.Delivered &&
                            x.Status != DeliveryAssignmentStatus.Failed)
                .OrderByDescending(x => x.AssignedAt)
        );

        if (activeAssignment == null)
        {
            return null;
        }

        return await MapToDtoWithDetailsAsync(activeAssignment);
    }

    /// <summary>
    /// جلب قائمة مشاوير المندوب الحالي مع الفلترة حسب الحالة والتقسيم
    /// </summary>
    public async Task<PagedResultDto<DeliveryAssignmentDto>> GetMyDeliveriesAsync(GetMyDeliveriesInput input)
    {
        var courier = await GetCurrentCourierEntityAsync();

        var query = (await _assignmentRepository.GetQueryableAsync())
            .Where(x => x.CourierId == courier.Id)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(DeliveryAssignment.AssignedAt)} desc"
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).PageBy(input)
        );

        var dtoList = new List<DeliveryAssignmentDto>();
        foreach (var item in items)
        {
            dtoList.Add(await MapToDtoWithDetailsAsync(item));
        }

        return new PagedResultDto<DeliveryAssignmentDto>(totalCount, dtoList);
    }

    /// <summary>
    /// جلب تفاصيل التوصيل لطلب معين بواسطة معرف الطلب
    /// </summary>
    public async Task<DeliveryAssignmentDto?> GetDeliveryByOrderIdAsync(Guid orderId)
    {
        var assignment = await AsyncExecuter.FirstOrDefaultAsync(
            (await _assignmentRepository.GetQueryableAsync())
                .Where(x => x.OrderId == orderId)
        );

        if (assignment == null)
        {
            return null;
        }

        return await MapToDtoWithDetailsAsync(assignment);
    }


    /// <summary>
    /// قبول المندوب للطلب المسند إليه
    /// </summary>
    [UnitOfWork]
    public async Task<DeliveryAssignmentDto> AcceptDeliveryAsync(Guid deliveryAssignmentId)
    {
        var assignment = await _assignmentRepository.GetAsync(deliveryAssignmentId);
        var courier = await GetCurrentCourierEntityAsync();

        if (assignment.CourierId != courier.Id)
        {
            throw new UserFriendlyException("غير مصرح لك بقبول هذا المشوار نظراً لعدم إسناده لحسابك.");
        }

        if (assignment.Status != DeliveryAssignmentStatus.Assigned)
        {
            throw new UserFriendlyException("لا يمكن قبول المشوار في حالته الحالية.");
        }

        assignment.Status = DeliveryAssignmentStatus.Accepted;
        assignment.AcceptedAt = Clock.Now;
        await _assignmentRepository.UpdateAsync(assignment, autoSave: true);

        // إشعار المتجر والعميل بقبول المندوب للمشوار
        var order = await _orderRepository.FindAsync(assignment.OrderId);
        if (order != null)
        {
            var store = await _storeRepository.FindAsync(order.StoreId);
            if (store != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: store.OwnerId,
                    notificationTypeName: "OrderCourierAccepted",
                    title: "المندوب وافق على المشوار",
                    message: $"وافق المندوب على توصيل الطلب #{order.OrderNumber} وهو في طريقه لاستلامه من متجركم.",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id,
                    actionUrl: $"/store/orders/{order.Id}"
                );
            }

            var customer = await _customerRepository.FindAsync(order.CustomerId);
            if (customer != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: customer.UserId,
                    notificationTypeName: "OrderStatusChanged",
                    title: "تم تعيين وتأكيد المندوب لطلبك",
                    message: $"وافق المندوب على توصيل طلبك #{order.OrderNumber} وجارٍ التوجه للمتجر لاستلامه.",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id,
                    actionUrl: $"/orders/{order.Id}"
                );
            }
        }

        Logger.LogInformation("المندوب {CourierId} قبل التوصيلة: {AssignmentId}", courier.Id, assignment.Id);

        return await MapToDtoWithDetailsAsync(assignment);
    }

    /// <summary>
    /// تسجيل استلام المندوب للطلب من المتجر والانطلاق به للعميل
    /// </summary>
    [UnitOfWork]
    public async Task<DeliveryAssignmentDto> PickupDeliveryAsync(Guid deliveryAssignmentId)
    {
        var assignment = await _assignmentRepository.GetAsync(deliveryAssignmentId);
        var courier = await GetCurrentCourierEntityAsync();

        if (assignment.CourierId != courier.Id)
        {
            throw new UserFriendlyException("غير مصرح لك بتحديث حالة هذا المشوار.");
        }

        assignment.Status = DeliveryAssignmentStatus.PickedUp;
        assignment.PickupAt = Clock.Now;
        await _assignmentRepository.UpdateAsync(assignment, autoSave: true);

        // تحديث حالة الطلب إلى (في الطريق / جاري التوصيل)
        var order = await _orderRepository.FindAsync(assignment.OrderId);
        if (order != null)
        {
            var outForDeliveryStatus = await _orderStatusRepository.FirstOrDefaultAsync(x =>
                x.Name == "OutForDelivery" || x.Name == "InTransit" || x.Name == "Delivery");

            if (outForDeliveryStatus != null && order.OrderStatusId != outForDeliveryStatus.Id)
            {
                var oldStatusId = order.OrderStatusId;
                order.OrderStatusId = outForDeliveryStatus.Id;
                await _orderRepository.UpdateAsync(order, autoSave: true);

                await _orderStatusHistoryRepository.InsertAsync(new OrderStatusHistory(
                    GuidGenerator.Create(),
                    order.Id,
                    outForDeliveryStatus.Id,
                    CurrentUser.GetId(),
                    "Courier",
                    oldStatusId,
                    "استلم المندوب الطلب وهو في الطريق إلى العميل"
                ), autoSave: true);
            }

            var customer = await _customerRepository.FindAsync(order.CustomerId);
            var store = await _storeRepository.FindAsync(order.StoreId);

            // إشعار العميل بأن الطلب في الطريق إليه
            if (customer != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: customer.UserId,
                    notificationTypeName: "OrderOutForDelivery",
                    title: "طلبك في الطريق إليك!",
                    message: $"المندوب استلم طلبك رقم #{order.OrderNumber} من متجر \"{store?.Name ?? "المتجر"}\" وهو في الطريق لتسليمه لك.",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id,
                    actionUrl: $"/orders/{order.Id}"
                );
            }

            // إشعار المتجر بانطلاق المندوب
            if (store != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: store.OwnerId,
                    notificationTypeName: "OrderOutForDelivery",
                    title: "خرج الطلب للتوصيل",
                    message: $"استلم المندوب الطلب #{order.OrderNumber} وانطلق في الطريق لتسليمه للعميل.",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id,
                    actionUrl: $"/store/orders/{order.Id}"
                );
            }
        }

        Logger.LogInformation("المندوب {CourierId} استلم الطلب وانطلق به: {AssignmentId}", courier.Id, assignment.Id);

        return await MapToDtoWithDetailsAsync(assignment);
    }

    /// <summary>
    /// توثيق وإثبات تأكيد تسليم الطلب النهائي للعميل
    /// </summary>
    [UnitOfWork]
    public async Task<DeliveryAssignmentDto> ConfirmDeliveryAsync(ConfirmDeliveryInput input)
    {
        var assignment = await _assignmentRepository.GetAsync(input.DeliveryAssignmentId);

        if (assignment.Status == DeliveryAssignmentStatus.Delivered)
        {
            throw new UserFriendlyException("تم تأكيد تسليم هذا الطلب مسبقاً.");
        }

        var courier = await _courierRepository.GetAsync(assignment.CourierId);

        // 1. إنشاء سجل تأكيد وإثبات التسليم
        var confirmation = new DeliveryConfirmation(
            GuidGenerator.Create(),
            assignment.Id,
            input.ConfirmedBy,
            CurrentUser.GetId(),
            input.VerificationCode,
            input.SignatureUrl,
            input.PhotoProofUrl,
            input.Notes
        );

        await _confirmationRepository.InsertAsync(confirmation, autoSave: true);

        // 2. تحديث سجل المشوار إلى مكتمل Delivered
        assignment.Status = DeliveryAssignmentStatus.Delivered;
        assignment.DeliveredAt = Clock.Now;
        await _assignmentRepository.UpdateAsync(assignment, autoSave: true);

        // 3. تحديث مؤشرات المندوب وإتاحته مجدداً
        courier.TotalDeliveries++;
        courier.IsAvailable = true;
        await _courierRepository.UpdateAsync(courier, autoSave: true);

        // 4. تحديث حالة الطلب إلى مكتمل / تم التسليم Delivered
        var order = await _orderRepository.FindAsync(assignment.OrderId);
        if (order != null)
        {
            var deliveredStatus = await _orderStatusRepository.FirstOrDefaultAsync(x =>
                x.Name == "Delivered" || x.Name == "Completed");

            if (deliveredStatus != null && order.OrderStatusId != deliveredStatus.Id)
            {
                var oldStatusId = order.OrderStatusId;
                order.OrderStatusId = deliveredStatus.Id;
                await _orderRepository.UpdateAsync(order, autoSave: true);

                await _orderStatusHistoryRepository.InsertAsync(new OrderStatusHistory(
                    GuidGenerator.Create(),
                    order.Id,
                    deliveredStatus.Id,
                    CurrentUser.GetId(),
                    "Courier",
                    oldStatusId,
                    "تم تسليم الطلب للعميل وتأكيد الاستلام"
                ), autoSave: true);
            }

            var customer = await _customerRepository.FindAsync(order.CustomerId);
            var store = await _storeRepository.FindAsync(order.StoreId);

            // إرسال إشعار تهنئة وتسليم للعميل ودعوته لتقييم المتجر
            if (customer != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: customer.UserId,
                    notificationTypeName: "OrderDelivered",
                    title: "تم تسليم طلبك بنجاح",
                    message: $"تم تسليم طلبك رقم #{order.OrderNumber} بنجاح. يسعدنا مشاركة تقييمك للمتجر!",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id,
                    actionUrl: $"/orders/{order.Id}/review"
                );
            }

            // إشعار المتجر باكتمال التسليم
            if (store != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: store.OwnerId,
                    notificationTypeName: "OrderDelivered",
                    title: "اكتمل تسليم الطلب",
                    message: $"تم تسليم الطلب #{order.OrderNumber} للعميل بنجاح واكتمال المشوار.",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id,
                    actionUrl: $"/store/orders/{order.Id}"
                );
            }

            // إشعار إدارة المنصة
            await _notificationSender.SendToAdminsAsync(
                notificationTypeName: "OrderDelivered",
                title: "اكتمال توصيل طلب",
                message: $"أكمل المندوب تسليم الطلب #{order.OrderNumber} لمتجر \"{store?.Name ?? "المتجر"}\" بنجاح.",
                relatedEntityName: "Order",
                relatedEntityId: order.Id,
                actionUrl: $"/admin/orders/{order.Id}"
            );
        }

        Logger.LogInformation(
            "تم تأكيد تسليم المشوار {AssignmentId} بنجاح بواسطة المستخدم {UserId}",
            assignment.Id, CurrentUser.GetId());

        return await MapToDtoWithDetailsAsync(assignment);
    }

    /// <summary>
    /// تسجيل تعثر أو فشل توصيل الطلب مع ذكر السبب
    /// </summary>
    [UnitOfWork]
    public async Task<DeliveryAssignmentDto> FailDeliveryAsync(Guid deliveryAssignmentId, FailDeliveryInput input)
    {
        var assignment = await _assignmentRepository.GetAsync(deliveryAssignmentId);

        if (assignment.Status == DeliveryAssignmentStatus.Delivered)
        {
            throw new UserFriendlyException("لا يمكن إلغاء أو تفشيل طلب تم تسليمه بالفعل.");
        }

        assignment.Status = DeliveryAssignmentStatus.Failed;
        await _assignmentRepository.UpdateAsync(assignment, autoSave: true);

        // إتاحة المندوب لاستقبال مشاوير أخرى
        var courier = await _courierRepository.FindAsync(assignment.CourierId);
        if (courier != null)
        {
            courier.IsAvailable = true;
            await _courierRepository.UpdateAsync(courier, autoSave: true);
        }

        var order = await _orderRepository.FindAsync(assignment.OrderId);
        if (order != null)
        {
            var customer = await _customerRepository.FindAsync(order.CustomerId);
            var store = await _storeRepository.FindAsync(order.StoreId);

            // إشعار العميل بتعثر التوصيل
            if (customer != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: customer.UserId,
                    notificationTypeName: "DeliveryFailed",
                    title: "تنبيه بخصوص توصيل طلبك",
                    message: $"تعذر تسليم طلبك #{order.OrderNumber}. السبب: {input.Reason}. يرجى التواصل مع المتجر للمتابعة.",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id,
                    actionUrl: $"/orders/{order.Id}"
                );
            }

            // إشعار المتجر بتعثر التوصيل
            if (store != null)
            {
                await _notificationSender.SendToUserAsync(
                    recipientUserId: store.OwnerId,
                    notificationTypeName: "DeliveryFailed",
                    title: "تعثر مشوار توصيل الطلب",
                    message: $"تعثر المندوب في تسليم الطلب #{order.OrderNumber} للعميل. سبب التعثر: {input.Reason}.",
                    relatedEntityName: "Order",
                    relatedEntityId: order.Id,
                    actionUrl: $"/store/orders/{order.Id}"
                );
            }

            // إشعار إدارة المنصة
            await _notificationSender.SendToAdminsAsync(
                notificationTypeName: "DeliveryFailed",
                title: "تعثر مشوار توصيل في المنصة",
                message: $"تعثر توصيل الطلب #{order.OrderNumber} لمتجر \"{store?.Name ?? "المتجر"}\". السبب: {input.Reason}.",
                relatedEntityName: "Order",
                relatedEntityId: order.Id,
                actionUrl: $"/admin/orders/{order.Id}"
            );
        }

        Logger.LogWarning(
            "تعثر توصيل المشوار {AssignmentId} بسبب: {Reason}",
            assignment.Id, input.Reason);

        return await MapToDtoWithDetailsAsync(assignment);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// تحويل كيان التعيين إلى كائن DTO وتضمين اسم المندوب وسجل التأكيد
    /// </summary>
    private async Task<DeliveryAssignmentDto> MapToDtoWithDetailsAsync(DeliveryAssignment assignment)
    {
        var dto = _mapper.ToDeliveryAssignmentDto(assignment);

        var courier = await _courierRepository.FindAsync(assignment.CourierId);
        if (courier != null)
        {
            var user = await _identityUserRepository.FindAsync(courier.UserId);
            if (user != null)
            {
                dto.CourierName = $"{user.Name} {user.Surname}".Trim();
                if (string.IsNullOrWhiteSpace(dto.CourierName))
                {
                    dto.CourierName = user.UserName;
                }
            }
        }

        var confirmation = await _confirmationRepository.FirstOrDefaultAsync(x => x.DeliveryAssignmentId == assignment.Id);
        if (confirmation != null)
        {
            dto.Confirmation = _mapper.ToDeliveryConfirmationDto(confirmation);
        }

        return dto;
    }

    /// <summary>
    /// استرجاع كيان المندوب الخاص بالمستخدم الحالي المسجل دخوله
    /// </summary>
    private async Task<Courier> GetCurrentCourierEntityAsync()
    {
        var currentUserId = CurrentUser.GetId();
        var courier = await _courierRepository.FirstOrDefaultAsync(x => x.UserId == currentUserId);
        if (courier == null)
        {
            throw new UserFriendlyException("الحساب الحالي غير مسجل كمندوب توصيل في المنصة.");
        }

        return courier;
    }

    /// <summary>
    /// استرجاع المندوب أو null
    /// </summary>
    private async Task<Courier?> GetCurrentCourierOrNullAsync()
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return null;
        }

        var currentUserId = CurrentUser.GetId();
        return await _courierRepository.FirstOrDefaultAsync(x => x.UserId == currentUserId);
    }

    #endregion
}
