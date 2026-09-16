using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Talabi.Customers;
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
using Volo.Abp.Users;

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
    private readonly Volo.Abp.Identity.IIdentityUserRepository _identityUserRepository;
    private readonly Talabi.Carts.IShoppingCartAppService _shoppingCartAppService;
    private readonly IRepository<Talabi.Deliveries.DeliveryAssignment, Guid> _deliveryAssignmentRepository;
    private readonly IRepository<Talabi.Deliveries.Courier, Guid> _courierRepository;
    private readonly IRepository<Talabi.Payments.Payment, Guid> _paymentRepository;
    private readonly IRepository<Talabi.Payments.PaymentReceipt, Guid> _paymentReceiptRepository;
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
        Volo.Abp.Identity.IIdentityUserRepository identityUserRepository,
        INotificationSender notificationSender,
        Talabi.Carts.IShoppingCartAppService shoppingCartAppService,
        IRepository<Talabi.Deliveries.DeliveryAssignment, Guid> deliveryAssignmentRepository,
        IRepository<Talabi.Deliveries.Courier, Guid> courierRepository,
        IRepository<Talabi.Payments.Payment, Guid> paymentRepository,
        IRepository<Talabi.Payments.PaymentReceipt, Guid> paymentReceiptRepository)
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
        _identityUserRepository = identityUserRepository;
        _notificationSender = notificationSender;
        _shoppingCartAppService = shoppingCartAppService;
        _deliveryAssignmentRepository = deliveryAssignmentRepository;
        _courierRepository = courierRepository;
        _paymentRepository = paymentRepository;
        _paymentReceiptRepository = paymentReceiptRepository;
        _orderMapper = new OrderMapper();
    }

    public async Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListInput input)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Store, x => x.OrderStatus, x => x.Customer);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.OrderNumber.Contains(input.Filter) || (x.CustomerNotes != null && x.CustomerNotes.Contains(input.Filter)));
        }

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

        if (input.PaymentStatus.HasValue)
        {
            query = query.Where(x => x.PaymentStatus == input.PaymentStatus.Value);
        }

        if (input.FromDate.HasValue)
        {
            query = query.Where(x => x.CreationTime >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            query = query.Where(x => x.CreationTime <= input.ToDate.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var orders = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        var paymentMethods = await _paymentMethodRepository.GetListAsync();
        var paymentMethodDict = paymentMethods.ToDictionary(p => p.Id, p => p.DisplayName);

        var customerUserIds = orders.Where(o => o.Customer != null).Select(o => o.Customer!.UserId).Distinct().ToList();
        var customerUsers = await _identityUserRepository.GetListByIdsAsync(customerUserIds);
        var userDict = customerUsers.ToDictionary(u => u.Id);

        var orderDtos = orders.Select(o => 
        {
            var dto = _orderMapper.MapToOrderDto(o);
            if (o.Customer != null && userDict.TryGetValue(o.Customer.UserId, out var iu))
            {
                dto.CustomerName = (iu.Name + " " + iu.Surname).Trim();
                dto.CustomerPhoneNumber = iu.PhoneNumber;
            }
            if (paymentMethodDict.TryGetValue(o.PaymentMethodId, out var pmName))
            {
                dto.PaymentMethodName = pmName;
            }
            return dto;
        }).ToList();

        return new PagedResultDto<OrderDto>(totalCount, orderDtos);
    }

    public async Task<OrderDto> GetAsync(Guid id)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Store, x => x.OrderStatus, x => x.Customer, x => x.Items, x => x.StatusHistories);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(Order), id);
        }

        return await EnrichOrderDtoAsync(order);
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
            var productQuery = await _productRepository.WithDetailsAsync(x => x.SalesUnits);
            var product = await AsyncExecuter.FirstOrDefaultAsync(productQuery.Where(x => x.Id == itemInput.ProductId));

            if (product == null)
            {
                throw new EntityNotFoundException(typeof(Product), itemInput.ProductId);
            }

            if (!product.IsAvailable || !product.IsActive)
            {
                throw new UserFriendlyException($"المنتج ({product.Name}) غير متوفر حالياً.");
            }

            if (product.StoreId != input.StoreId)
            {
                throw new UserFriendlyException($"المنتج ({product.Name}) لا يتبع للمتجر المختار.");
            }

            // تحديد وحدة البيع والسعر
            Guid? salesUnitId = null;
            string unitName = !string.IsNullOrWhiteSpace(product.Unit) ? product.Unit : "حبة";
            decimal unitPrice = product.Price;
            decimal discount = product.Price - product.FinalPrice;

            if (itemInput.SalesUnitId.HasValue)
            {
                var matchedUnit = product.SalesUnits.FirstOrDefault(u => u.SalesUnitId == itemInput.SalesUnitId.Value && u.IsActive);
                if (matchedUnit == null)
                {
                    throw new UserFriendlyException($"وحدة البيع المحددة غير متوفرة للمنتج ({product.Name}).");
                }

                salesUnitId = matchedUnit.SalesUnitId;
                unitName = matchedUnit.UnitName;
                if (matchedUnit.Price.HasValue && matchedUnit.Price.Value > 0)
                {
                    unitPrice = matchedUnit.Price.Value;
                    discount = 0; // السعر مخصص لوحدة البيع
                }
            }
            else if (product.SalesUnits?.Count > 0)
            {
                var defaultUnit = product.SalesUnits.FirstOrDefault(u => u.IsDefault && u.IsActive)
                                  ?? product.SalesUnits.FirstOrDefault(u => u.IsActive);
                if (defaultUnit != null)
                {
                    salesUnitId = defaultUnit.SalesUnitId;
                    unitName = defaultUnit.UnitName;
                    if (defaultUnit.Price.HasValue && defaultUnit.Price.Value > 0)
                    {
                        unitPrice = defaultUnit.Price.Value;
                        discount = 0;
                    }
                }
            }

            var orderItem = new OrderItem(
                id: GuidGenerator.Create(),
                orderId: Guid.Empty, // سيتم ربطها لاحقاً
                productId: product.Id,
                productName: product.Name,
                sku: product.SKU,
                unit: unitName,
                quantity: itemInput.Quantity,
                unitPrice: unitPrice,
                discount: discount,
                notes: itemInput.Notes,
                productImageUrl: product.MainImageUrl,
                salesUnitId: salesUnitId
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

        // 1. إرسال إشعار تفصيلي وتنبيه فوري لمالك المتجر لمراجعة الطلب والإيصال والمنتجات
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "NewOrderReceived",
            title: "طلب جديد وارد بحاجة لمراجعتك",
            message: $"وصلك طلب جديد رقم #{order.OrderNumber} بقيمة {order.FinalAmount:N2} ر.ي يتضمن ({order.Items.Count}) منتجات. يرجى مراجعة تفاصيل المنتجات ومطابقة الإيصال المالي (إن وجد) لاتخاذ قرار القبول أو الرفض.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/store/orders/{order.Id}"
        );

        // 2. إرسال إشعار لحظي لإدارة المنصة
        await _notificationSender.SendToAdminsAsync(
            notificationTypeName: "NewOrderReceived",
            title: "طلب جديد في المنصة",
            message: $"تم تسجيل طلب جديد برقم #{order.OrderNumber} لمتجر \"{store.Name}\" بقيمة {order.FinalAmount:N2} ر.ي.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/admin/orders/{order.Id}"
        );

        // 3. إرسال إشعار تأكيد للعميل
        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderStatusChanged",
            title: "تم استلام طلبك بنجاح",
            message: $"تم استلام طلبك برقم #{order.OrderNumber} بنجاح، وهو حالياً بانتظار مراجعة وقبول المتجر.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/orders/{order.Id}"
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
                unit: itemDto.UnitName,
                quantity: itemDto.Quantity,
                unitPrice: itemDto.OriginalUnitPrice,
                discount: itemDto.UnitDiscount,
                notes: itemDto.Notes,
                productImageUrl: itemDto.ProductImageUrl,
                salesUnitId: itemDto.SalesUnitId
            );
            order.Items.Add(orderItem);
        }

        await _orderRepository.InsertAsync(order);

        // إنشاء عملية الدفع المرتبطة بالطلب
        var payment = new Talabi.Payments.Payment(
            id: GuidGenerator.Create(),
            orderId: order.Id,
            paymentMethodId: input.PaymentMethodId,
            amount: order.FinalAmount
        );
        await _paymentRepository.InsertAsync(payment);

        var history = new OrderStatusHistory(
            id: GuidGenerator.Create(),
            orderId: order.Id,
            toStatusId: pendingStatus.Id,
            changedByUserId: CurrentUser.Id ?? Guid.Empty,
            changedByRole: "Customer",
            notes: "تم إنشاء الطلب وهو بانتظار موافقة المتجر"
        );
        await _orderStatusHistoryRepository.InsertAsync(history);

        // 1. إرسال إشعار تفصيلي وتنبيه فوري لمالك المتجر لمراجعة الطلب والإيصال والمنتجات
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "NewOrderReceived",
            title: "طلب جديد وارد بحاجة لمراجعتك",
            message: $"وصلك طلب جديد رقم #{order.OrderNumber} بقيمة {order.FinalAmount:N2} ر.ي يتضمن ({order.Items.Count}) منتجات. يرجى مراجعة تفاصيل المنتجات ومطابقة الإيصال المالي (إن وجد) لاتخاذ قرار القبول أو الرفض.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/store/orders/{order.Id}"
        );

        // 2. إرسال إشعار لحظي لإدارة المنصة
        await _notificationSender.SendToAdminsAsync(
            notificationTypeName: "NewOrderReceived",
            title: "طلب جديد في المنصة",
            message: $"تم تسجيل طلب جديد برقم #{order.OrderNumber} لمتجر \"{store.Name}\" بقيمة {order.FinalAmount:N2} ر.ي.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/admin/orders/{order.Id}"
        );

        // 3. إرسال إشعار تأكيد للعميل
        var customer = await _customerRepository.GetAsync(cartDto.CustomerId);
        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderStatusChanged",
            title: "تم استلام طلبك بنجاح",
            message: $"تم استلام طلبك برقم #{order.OrderNumber} بنجاح، وهو حالياً بانتظار مراجعة وقبول المتجر.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/orders/{order.Id}"
        );

        // تفريغ السلة بعد نجاح إنشاء الطلب
        await _shoppingCartAppService.ClearCartAsync();

        var dto = _orderMapper.MapToOrderDto(order);
        dto.Items = order.Items.Select(x => _orderMapper.MapToOrderItemDto(x)).ToList();
        return dto;
    }


    [UnitOfWork]
    public async Task<OrderDto> AttachPaymentReceiptAsync(Guid id, string receiptUrl, Guid? mediaFileId = null)
    {
        var order = await _orderRepository.GetAsync(id);
        order.PaymentReceiptUrl = receiptUrl;
        await _orderRepository.UpdateAsync(order);

        // ربط أو إنشاء عملية الدفع للطلب
        var payment = await _paymentRepository.FirstOrDefaultAsync(x => x.OrderId == id);
        if (payment == null)
        {
            payment = new Talabi.Payments.Payment(
                id: GuidGenerator.Create(),
                orderId: order.Id,
                paymentMethodId: order.PaymentMethodId,
                amount: order.FinalAmount
            );
            await _paymentRepository.InsertAsync(payment);
        }

        // ربط أو تحديث إيصال الدفع
        var receipt = await _paymentReceiptRepository.FirstOrDefaultAsync(x => x.PaymentId == payment.Id);
        if (receipt == null)
        {
            receipt = new Talabi.Payments.PaymentReceipt(
                id: GuidGenerator.Create(),
                paymentId: payment.Id,
                mediaFileId: mediaFileId ?? Guid.Empty,
                amount: order.FinalAmount
            );
            await _paymentReceiptRepository.InsertAsync(receipt);
        }
        else
        {
            receipt.Resubmit(
                mediaFileId: mediaFileId ?? receipt.MediaFileId,
                amount: order.FinalAmount
            );
            await _paymentReceiptRepository.UpdateAsync(receipt);
        }

        // إشعار صاحب المتجر بوصول الإيصال للمراجعة
        var store = await _storeRepository.GetAsync(order.StoreId);
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "PaymentReceiptSubmitted",
            title: "إيصال دفع جديد",
            message: $"تم إرفاق إيصال دفع جديد للطلب رقم {order.OrderNumber} وهو بانتظار المراجعة والتحقق.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

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
        var courier = await _courierRepository.GetAsync(courierId);

        order.CourierId = courierId;
        await _orderRepository.UpdateAsync(order);

        // إنشاء أو تحديث سجل تعيين التوصيل لتوثيق المشوار
        var assignment = await _deliveryAssignmentRepository.FirstOrDefaultAsync(x => x.OrderId == id);
        if (assignment == null)
        {
            assignment = new Talabi.Deliveries.DeliveryAssignment(
                GuidGenerator.Create(),
                id,
                courierId,
                CurrentUser.GetId()
            );
            await _deliveryAssignmentRepository.InsertAsync(assignment, autoSave: true);
        }
        else
        {
            assignment.CourierId = courierId;
            assignment.AssignedByUserId = CurrentUser.GetId();
            assignment.Status = Talabi.Deliveries.DeliveryAssignmentStatus.Assigned;
            assignment.AssignedAt = Clock.Now;
            await _deliveryAssignmentRepository.UpdateAsync(assignment, autoSave: true);
        }

        // جعل المندوب مشغولاً
        courier.IsAvailable = false;
        var store = await _storeRepository.GetAsync(order.StoreId);

        // إشعار السائق/المندوب بأنه تم تكليفه بالمشوار
        await _notificationSender.SendToUserAsync(
            recipientUserId: courier.UserId,
            notificationTypeName: "OrderAssignedToCourier",
            title: "مشوار توصيل جديد",
            message: $"تم تكليفك بتوصيل الطلب #{order.OrderNumber} من متجر \"{store.Name}\". يرجى التوجه للتطبيق لقبول المشوار.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: "/courier/deliveries"
        );

        // إشعار صاحب المتجر بإسناد الطلب للمندوب
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "OrderStatusChanged",
            title: "تم تعيين مندوب للطلب",
            message: $"تم إسناد توصيل الطلب #{order.OrderNumber} للمندوب، وبانتظار قبوله للمشوار والتوجه لاستلامه.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/store/orders/{order.Id}"
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
        var reasonObj = await _cancellationReasonRepository.FindAsync(reasonId.Value);
        var reasonText = !string.IsNullOrWhiteSpace(input.AdditionalNotes)
            ? input.AdditionalNotes
            : reasonObj?.Reason ?? "طلب من العميل";

        // إشعار صاحب المتجر بإلغاء الطلب مع توضيح السبب
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "OrderCancelled",
            title: "إلغاء الطلب من العميل",
            message: $"قام العميل بإلغاء الطلب #{order.OrderNumber}. سبب الإلغاء: {reasonText}.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/store/orders/{order.Id}"
        );

        // إشعار إدارة المنصة
        await _notificationSender.SendToAdminsAsync(
            notificationTypeName: "OrderCancelled",
            title: "إلغاء طلب في المنصة",
            message: $"ألغى العميل الطلب #{order.OrderNumber} التابع لمتجر \"{store.Name}\". السبب: {reasonText}.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/admin/orders/{order.Id}"
        );

        return await GetAsync(id);
    }

    [UnitOfWork]
    public async Task<OrderDto> AcceptOrderAsync(Guid id, AcceptOrderInput? input = null)
    {
        var order = await _orderRepository.GetAsync(id);
        var oldStatusId = order.OrderStatusId;

        var acceptedStatus = await _orderStatusRepository.FirstOrDefaultAsync(x => x.Name == "Accepted" || x.Name == "Preparing");
        if (acceptedStatus == null) throw new BusinessException("Target status not found.");

        order.OrderStatusId = acceptedStatus.Id;
        order.ConfirmedAt = DateTime.UtcNow;

        if (input != null)
        {
            if (input.EstimatedDeliveryTime.HasValue)
            {
                order.EstimatedDeliveryTime = input.EstimatedDeliveryTime.Value;
            }
            else if (input.EstimatedMinutes.HasValue && input.EstimatedMinutes.Value > 0)
            {
                order.EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(input.EstimatedMinutes.Value);
            }
        }

        await _orderRepository.UpdateAsync(order);

        var history = new OrderStatusHistory(
            GuidGenerator.Create(),
            order.Id,
            acceptedStatus.Id,
            CurrentUser.Id ?? Guid.Empty,
            "Store",
            oldStatusId,
            input?.Notes ?? "تم قبول الطلب من قبل المتجر"
        );
        await _orderStatusHistoryRepository.InsertAsync(history);

        var customer = await _customerRepository.GetAsync(order.CustomerId);
        var store = await _storeRepository.GetAsync(order.StoreId);

        string timeInfo = order.EstimatedDeliveryTime.HasValue 
            ? $" والوقت التقديري المتوقع للتسليم: {order.EstimatedDeliveryTime.Value:hh:mm tt}" 
            : string.Empty;

        // إشعار العميل بقبول وتجهيز طلبه
        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderAccepted",
            title: "تم قبول وتجهيز طلبك!",
            message: $"وافق متجر \"{store.Name}\" على طلبك رقم #{order.OrderNumber}، وجارٍ تحضيره وتجهيزه للتسليم.{timeInfo}",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/orders/{order.Id}"
        );

        // إشعار إدارة المنصة بقبول الطلب
        await _notificationSender.SendToAdminsAsync(
            notificationTypeName: "OrderAccepted",
            title: "قبول طلب من المتجر",
            message: $"وافق متجر \"{store.Name}\" على تلبية الطلب #{order.OrderNumber}.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/admin/orders/{order.Id}"
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
        var store = await _storeRepository.GetAsync(order.StoreId);
        var reasonObj = await _cancellationReasonRepository.FindAsync(reasonId.Value);
        var reasonText = !string.IsNullOrWhiteSpace(input.AdditionalNotes)
            ? input.AdditionalNotes
            : reasonObj?.Reason ?? "اعتذار المتجر عن تلبية الطلب حالياً";

        // إشعار العميل بالرفض مع توضيح سبب الرفض بالتفصيل
        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderRejected",
            title: "نعتذر، تم رفض طلبك",
            message: $"نعتذر منك، اعتذر متجر \"{store.Name}\" عن قبول طلبك #{order.OrderNumber}. سبب الرفض: {reasonText}.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/orders/{order.Id}"
        );

        // إشعار إدارة المنصة برفض الطلب
        await _notificationSender.SendToAdminsAsync(
            notificationTypeName: "OrderRejected",
            title: "رفض طلب من المتجر",
            message: $"اعتذر متجر \"{store.Name}\" عن تلبية الطلب #{order.OrderNumber}. السبب: {reasonText}.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/admin/orders/{order.Id}"
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
        var store = await _storeRepository.GetAsync(order.StoreId);

        // إشعار العميل بالتسليم ودعوته لتقييم المتجر
        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderDelivered",
            title: "تم تسليم طلبك بنجاح",
            message: $"تم تسليم طلبك #{order.OrderNumber} من متجر \"{store.Name}\" بنجاح. نتمنى أن ينال إعجابك! يسعدنا مشاركة تقييمك للمتجر.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/orders/{order.Id}/review"
        );

        // إشعار صاحب المتجر
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "OrderDelivered",
            title: "اكتمل تسليم الطلب",
            message: $"تم تسليم الطلب #{order.OrderNumber} للعميل بنجاح واكتمال المعاملة.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/store/orders/{order.Id}"
        );

        // إشعار إدارة المنصة
        await _notificationSender.SendToAdminsAsync(
            notificationTypeName: "OrderDelivered",
            title: "اكتمال تسليم طلب",
            message: $"تم تسليم الطلب #{order.OrderNumber} التابع لمتجر \"{store.Name}\" بنجاح.",
            relatedEntityName: "Order",
            relatedEntityId: order.Id,
            actionUrl: $"/admin/orders/{order.Id}"
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
    /// جلب قائمة طلبات العميل الحالي مع الفلترة حسب (الكل، النشطة، السابقة)
    /// </summary>
    [Authorize]
    public async Task<PagedResultDto<OrderDto>> GetMyOrdersAsync(GetMyOrdersInput input)
    {
        var currentUserId = CurrentUser.GetId();
        var customer = await _customerRepository.FirstOrDefaultAsync(x => x.UserId == currentUserId);
        if (customer == null)
        {
            return new PagedResultDto<OrderDto>(0, new List<OrderDto>());
        }

        var statuses = await _orderStatusRepository.GetListAsync();
        var statusDict = statuses.ToDictionary(x => x.Id, x => x);

        var activeStatusNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Pending", "Accepted", "Preparing", "InTransit"
        };
        var pastStatusNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Delivered", "Cancelled", "Rejected"
        };

        var activeStatusIds = statuses.Where(s => activeStatusNames.Contains(s.Name)).Select(s => s.Id).ToList();
        var pastStatusIds = statuses.Where(s => pastStatusNames.Contains(s.Name)).Select(s => s.Id).ToList();

        var query = await _orderRepository.WithDetailsAsync(x => x.Store, x => x.OrderStatus, x => x.Customer, x => x.Items);
        query = query.Where(x => x.CustomerId == customer.Id);

        if (input.FilterType == OrderFilterType.Active)
        {
            query = query.Where(x => activeStatusIds.Contains(x.OrderStatusId));
        }
        else if (input.FilterType == OrderFilterType.Past)
        {
            query = query.Where(x => pastStatusIds.Contains(x.OrderStatusId));
        }

        if (input.OrderStatusId.HasValue)
        {
            query = query.Where(x => x.OrderStatusId == input.OrderStatusId.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var orders = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        var paymentMethods = await _paymentMethodRepository.GetListAsync();
        var paymentMethodDict = paymentMethods.ToDictionary(p => p.Id, p => p.DisplayName);

        var customerUserIds = orders.Where(o => o.Customer != null).Select(o => o.Customer!.UserId).Distinct().ToList();
        var customerUsers = await _identityUserRepository.GetListByIdsAsync(customerUserIds);
        var userDict = customerUsers.ToDictionary(u => u.Id);

        var orderDtos = orders.Select(o => 
        {
            var dto = _orderMapper.MapToOrderDto(o);
            if (o.Items != null && o.Items.Any())
            {
                dto.Items = o.Items.Select(x => _orderMapper.MapToOrderItemDto(x)).ToList();
            }
            if (o.Customer != null && userDict.TryGetValue(o.Customer.UserId, out var iu))
            {
                dto.CustomerName = (iu.Name + " " + iu.Surname).Trim();
                dto.CustomerPhoneNumber = iu.PhoneNumber;
            }
            if (paymentMethodDict.TryGetValue(o.PaymentMethodId, out var pmName))
            {
                dto.PaymentMethodName = pmName;
            }
            return dto;
        }).ToList();

        return new PagedResultDto<OrderDto>(totalCount, orderDtos);
    }

    /// <summary>
    /// جلب إحصائيات ومؤشرات طلبات العميل الحالي
    /// </summary>
    [Authorize]
    public async Task<CustomerOrderStatsDto> GetMyOrderStatsAsync()
    {
        var currentUserId = CurrentUser.GetId();
        var customer = await _customerRepository.FirstOrDefaultAsync(x => x.UserId == currentUserId);
        if (customer == null)
        {
            return new CustomerOrderStatsDto();
        }

        var statuses = await _orderStatusRepository.GetListAsync();
        var activeStatusNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Pending", "Accepted", "Preparing", "InTransit" };
        var activeStatusIds = statuses.Where(s => activeStatusNames.Contains(s.Name)).Select(s => s.Id).ToList();

        var deliveredStatus = statuses.FirstOrDefault(s => s.Name.Equals("Delivered", StringComparison.OrdinalIgnoreCase));
        var cancelledStatusNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Cancelled", "Rejected" };
        var cancelledStatusIds = statuses.Where(s => cancelledStatusNames.Contains(s.Name)).Select(s => s.Id).ToList();

        var query = await _orderRepository.GetQueryableAsync();
        var customerOrders = query.Where(x => x.CustomerId == customer.Id);

        var totalOrders = await AsyncExecuter.CountAsync(customerOrders);
        var activeOrders = await AsyncExecuter.CountAsync(customerOrders.Where(x => activeStatusIds.Contains(x.OrderStatusId)));
        var deliveredOrders = deliveredStatus != null 
            ? await AsyncExecuter.CountAsync(customerOrders.Where(x => x.OrderStatusId == deliveredStatus.Id)) 
            : 0;
        var cancelledOrders = await AsyncExecuter.CountAsync(customerOrders.Where(x => cancelledStatusIds.Contains(x.OrderStatusId)));
        
        var deliveredQuery = deliveredStatus != null 
            ? customerOrders.Where(x => x.OrderStatusId == deliveredStatus.Id) 
            : customerOrders.Where(x => false);
        
        var deliveredAmounts = await AsyncExecuter.ToListAsync(deliveredQuery.Select(x => x.FinalAmount));
        var totalSpent = deliveredAmounts.Sum();

        return new CustomerOrderStatsDto
        {
            TotalOrders = totalOrders,
            ActiveOrdersCount = activeOrders,
            DeliveredOrdersCount = deliveredOrders,
            CancelledOrdersCount = cancelledOrders,
            TotalSpent = totalSpent
        };
    }

    /// <summary>
    /// جلب إحصائيات ومؤشرات أداء طلبات المتجر والمبيعات
    /// </summary>
    [Authorize]
    public async Task<StoreOrderStatsDto> GetStoreOrderStatsAsync(Guid storeId)
    {
        var statuses = await _orderStatusRepository.GetListAsync();
        var statusByName = statuses.ToDictionary(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase);

        var query = await _orderRepository.GetQueryableAsync();
        var storeOrders = query.Where(x => x.StoreId == storeId);

        var totalOrders = await AsyncExecuter.CountAsync(storeOrders);

        Guid GetStatusId(string name) => statusByName.TryGetValue(name, out var id) ? id : Guid.Empty;

        var pendingId = GetStatusId("Pending");
        var acceptedId = GetStatusId("Accepted");
        var preparingId = GetStatusId("Preparing");
        var inTransitId = GetStatusId("InTransit");
        var deliveredId = GetStatusId("Delivered");
        var cancelledId = GetStatusId("Cancelled");
        var rejectedId = GetStatusId("Rejected");

        var pendingCount = pendingId != Guid.Empty ? await AsyncExecuter.CountAsync(storeOrders.Where(x => x.OrderStatusId == pendingId)) : 0;
        var acceptedCount = acceptedId != Guid.Empty ? await AsyncExecuter.CountAsync(storeOrders.Where(x => x.OrderStatusId == acceptedId)) : 0;
        var preparingCount = preparingId != Guid.Empty ? await AsyncExecuter.CountAsync(storeOrders.Where(x => x.OrderStatusId == preparingId)) : 0;
        var inTransitCount = inTransitId != Guid.Empty ? await AsyncExecuter.CountAsync(storeOrders.Where(x => x.OrderStatusId == inTransitId)) : 0;
        var deliveredCount = deliveredId != Guid.Empty ? await AsyncExecuter.CountAsync(storeOrders.Where(x => x.OrderStatusId == deliveredId)) : 0;
        var cancelledCount = cancelledId != Guid.Empty ? await AsyncExecuter.CountAsync(storeOrders.Where(x => x.OrderStatusId == cancelledId)) : 0;
        var rejectedCount = rejectedId != Guid.Empty ? await AsyncExecuter.CountAsync(storeOrders.Where(x => x.OrderStatusId == rejectedId)) : 0;

        var today = DateTime.UtcNow.Date;
        var todayOrdersQuery = storeOrders.Where(x => x.CreationTime >= today);
        var todayCount = await AsyncExecuter.CountAsync(todayOrdersQuery);

        var deliveredOrdersQuery = deliveredId != Guid.Empty 
            ? storeOrders.Where(x => x.OrderStatusId == deliveredId) 
            : storeOrders.Where(x => false);
        var deliveredAmounts = await AsyncExecuter.ToListAsync(deliveredOrdersQuery.Select(x => x.FinalAmount));
        var totalRevenue = deliveredAmounts.Sum();

        var todayRevenueQuery = deliveredId != Guid.Empty 
            ? storeOrders.Where(x => x.OrderStatusId == deliveredId && x.CreationTime >= today) 
            : storeOrders.Where(x => false);
        var todayAmounts = await AsyncExecuter.ToListAsync(todayRevenueQuery.Select(x => x.FinalAmount));
        var todayRevenue = todayAmounts.Sum();

        var avgOrder = deliveredCount > 0 ? Math.Round(totalRevenue / deliveredCount, 2) : 0;

        return new StoreOrderStatsDto
        {
            TotalOrders = totalOrders,
            PendingOrdersCount = pendingCount,
            AcceptedOrdersCount = acceptedCount,
            PreparingOrdersCount = preparingCount,
            InTransitOrdersCount = inTransitCount,
            DeliveredOrdersCount = deliveredCount,
            CancelledOrdersCount = cancelledCount,
            RejectedOrdersCount = rejectedCount,
            TodayOrdersCount = todayCount,
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            AverageOrderValue = avgOrder
        };
    }

    /// <summary>
    /// اعتماد وتأكيد استلام الدفعة المالية للطلب من قبل المتجر بعد مراجعة الإيصال
    /// </summary>
    [Authorize]
    [UnitOfWork]
    public async Task<OrderDto> ConfirmOrderPaymentAsync(Guid id, ConfirmOrderPaymentInput? input = null)
    {
        var order = await _orderRepository.GetAsync(id);

        if (order.PaymentStatus == OrderPaymentStatus.Paid)
        {
            throw new UserFriendlyException("الطلب مدفوع بالفعل مسبقاً.");
        }

        order.PaymentStatus = OrderPaymentStatus.Paid;
        await _orderRepository.UpdateAsync(order);

        // إرسال إشعار للعميل باعتماد الدفعة
        var customer = await _customerRepository.GetAsync(order.CustomerId);
        await _notificationSender.SendToUserAsync(
            recipientUserId: customer.UserId,
            notificationTypeName: "OrderStatusChanged",
            title: "تم اعتماد الدفع بنجاح",
            message: $"قام المتجر بالتحقق من إيصال الدفع واعتماده لطلبك #{order.OrderNumber}." + (!string.IsNullOrEmpty(input?.Notes) ? $" ملاحظة: {input.Notes}" : string.Empty),
            relatedEntityName: "Order",
            relatedEntityId: order.Id
        );

        return await GetAsync(id);
    }

    /// <summary>
    /// إعادة الطلب السابق ونقل كافة أصنافه مباشرة إلى سلة المشتريات الحالية
    /// </summary>
    [Authorize]
    [UnitOfWork]
    public async Task<Talabi.Carts.Dtos.ShoppingCartDto> ReorderAsync(Guid id)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Items);
        var existingOrder = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
        if (existingOrder == null)
        {
            throw new EntityNotFoundException(typeof(Order), id);
        }

        if (existingOrder.Items == null || !existingOrder.Items.Any())
        {
            throw new UserFriendlyException("لا يحتوي هذا الطلب على أي منتجات لإعادة طلبها.");
        }

        foreach (var item in existingOrder.Items)
        {
            var product = await _productRepository.FirstOrDefaultAsync(p => p.Id == item.ProductId);
            if (product != null && product.IsActive)
            {
                await _shoppingCartAppService.AddItemAsync(new Talabi.Carts.Dtos.AddShoppingCartItemInput
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Notes = item.Notes
                });
            }
        }

        return await _shoppingCartAppService.GetMyCartAsync();
    }

    /// <summary>
    /// جلب المسار والخط الزمني لحالات الطلب
    /// </summary>
    public async Task<List<OrderStatusHistoryDto>> GetOrderTimelineAsync(Guid id)
    {
        var histories = await _orderStatusHistoryRepository.GetListAsync(x => x.OrderId == id);
        var statuses = await _orderStatusRepository.GetListAsync();
        var statusDict = statuses.ToDictionary(x => x.Id, x => x);

        return histories.OrderBy(x => x.CreationTime).Select(h => new OrderStatusHistoryDto
        {
            Id = h.Id,
            OrderId = h.OrderId,
            StatusId = h.ToStatusId,
            StatusName = statusDict.TryGetValue(h.ToStatusId, out var s) ? s.Name : string.Empty,
            StatusDisplayName = statusDict.TryGetValue(h.ToStatusId, out var sd) ? sd.DisplayName : string.Empty,
            StatusColor = statusDict.TryGetValue(h.ToStatusId, out var sc) ? sc.Color : null,
            StatusIcon = statusDict.TryGetValue(h.ToStatusId, out var si) ? si.Icon : null,
            ChangedByRole = h.ChangedByRole,
            Notes = h.Notes,
            CreationTime = h.CreationTime
        }).ToList();
    }

    /// <summary>
    /// حذف الطلب من السجل (حذف ناعم Soft Delete مع التحقق من عدم كونه نشطاً)
    /// </summary>
    [Authorize]
    [UnitOfWork]
    public async Task DeleteAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        var currentStatus = await _orderStatusRepository.GetAsync(order.OrderStatusId);

        // منع حذف الطلبات التي لا تزال جارية ونشطة لحماية النظام والعميل
        var activeStatusNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Pending", "Accepted", "Preparing", "InTransit"
        };

        if (activeStatusNames.Contains(currentStatus.Name))
        {
            throw new UserFriendlyException($"لا يمكن حذف الطلب #{order.OrderNumber} وهو في حالة نشطة ({currentStatus.DisplayName}). يرجى إلغاء الطلب أولاً إذا كان متاحاً.");
        }

        // الحذف الناعم (Soft Delete) المعياري في ABP
        await _orderRepository.DeleteAsync(order);
    }

    /// <summary>
    /// إثراء كائن عرض الطلب بالبيانات الكاملة (اسم العميل، رقم الهاتف، طريقة الدفع، والخط الزمني)
    /// </summary>
    private async Task<OrderDto> EnrichOrderDtoAsync(Order order)
    {
        var dto = _orderMapper.MapToOrderDto(order);

        if (order.Items != null && order.Items.Any())
        {
            dto.Items = order.Items.Select(x => _orderMapper.MapToOrderItemDto(x)).ToList();
        }

        var targetCustomer = order.Customer;
        if (targetCustomer == null && order.CustomerId != Guid.Empty)
        {
            targetCustomer = await _customerRepository.FindAsync(order.CustomerId);
        }

        if (targetCustomer != null)
        {
            var identityUser = await _identityUserRepository.FindAsync(targetCustomer.UserId);
            if (identityUser != null)
            {
                dto.CustomerName = (identityUser.Name + " " + identityUser.Surname).Trim();
                dto.CustomerPhoneNumber = identityUser.PhoneNumber;
            }
        }

        if (order.PaymentMethodId != Guid.Empty)
        {
            var paymentMethod = await _paymentMethodRepository.FindAsync(order.PaymentMethodId);
            if (paymentMethod != null)
            {
                dto.PaymentMethodName = paymentMethod.DisplayName;
            }
        }

        if (order.StatusHistories != null && order.StatusHistories.Any())
        {
            var statuses = await _orderStatusRepository.GetListAsync();
            var statusDict = statuses.ToDictionary(x => x.Id, x => x);

            dto.Timeline = order.StatusHistories
                .OrderBy(h => h.CreationTime)
                .Select(h => new OrderStatusHistoryDto
                {
                    Id = h.Id,
                    OrderId = h.OrderId,
                    StatusId = h.ToStatusId,
                    StatusName = statusDict.TryGetValue(h.ToStatusId, out var s) ? s.Name : string.Empty,
                    StatusDisplayName = statusDict.TryGetValue(h.ToStatusId, out var sd) ? sd.DisplayName : string.Empty,
                    StatusColor = statusDict.TryGetValue(h.ToStatusId, out var sc) ? sc.Color : null,
                    StatusIcon = statusDict.TryGetValue(h.ToStatusId, out var si) ? si.Icon : null,
                    ChangedByRole = h.ChangedByRole,
                    Notes = h.Notes,
                    CreationTime = h.CreationTime
                }).ToList();
        }

        return dto;
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
