using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Talabi.Customers;
using Talabi.Deliveries;
using Talabi.Deliveries.Dtos;
using Talabi.Notifications;
using Talabi.Orders;
using Talabi.Orders.Dtos;
using Talabi.Payments;
using Talabi.Products;
using Talabi.Stores;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace Talabi.EntityFrameworkCore.Applications;

/// <summary>
/// اختبارات وحدة وتكامل التحقق من توليد ودقة الإشعارات التلقائية
/// عبر جميع مسارات الطلب والتوصيل والمتاجر والعملاء
/// </summary>
[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class NotificationWorkflowTests : TalabiEntityFrameworkCoreTestBase
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public NotificationWorkflowTests()
    {
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _currentUser = GetRequiredService<ICurrentUser>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private async Task WithUowAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(new AbpUnitOfWorkOptions());
        await action(scope.ServiceProvider);
        await uow.CompleteAsync();
    }

    private async Task<TResult> WithUowAsync<TResult>(Func<IServiceProvider, Task<TResult>> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(new AbpUnitOfWorkOptions());
        var result = await action(scope.ServiceProvider);
        await uow.CompleteAsync();
        return result;
    }

    private IDisposable ChangeUser(Guid userId, string userName = "test_user")
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, userName),
            new Claim(AbpClaimTypes.Email, $"{userName}@talabi.com")
        }));
        return _currentPrincipalAccessor.Change(principal);
    }

    private async Task<(Store store, Customer customer, Product product, PaymentMethod paymentMethod, OrderStatus pendingStatus)>
        SetupOrderPrerequisitesAsync(Guid storeOwnerId, Guid customerUserId)
    {
        return await WithUowAsync(async sp =>
        {
            var storeTypeRepo = sp.GetRequiredService<IRepository<StoreType, Guid>>();
            var storeRepo = sp.GetRequiredService<IRepository<Store, Guid>>();
            var customerRepo = sp.GetRequiredService<IRepository<Customer, Guid>>();
            var productRepo = sp.GetRequiredService<IRepository<Product, Guid>>();
            var paymentMethodRepo = sp.GetRequiredService<IRepository<PaymentMethod, Guid>>();
            var orderStatusRepo = sp.GetRequiredService<IRepository<OrderStatus, Guid>>();

            var storeType = await storeTypeRepo.FirstOrDefaultAsync(x => x.Name == "مطاعم");
            if (storeType == null)
            {
                storeType = new StoreType(_guidGenerator.Create(), "مطاعم", 1, true);
                await storeTypeRepo.InsertAsync(storeType, autoSave: true);
            }

            var store = new Store(
                _guidGenerator.Create(),
                storeOwnerId,
                storeType.Id,
                "مطعم الأصالة " + Guid.NewGuid().ToString("N")[..4],
                "alasala-" + Guid.NewGuid().ToString("N")[..8],
                "770000000",
                "صنعاء - حدة",
                15.35M,
                44.20M
            );
            store.Status = StoreStatus.Active;
            store.IsActive = true;
            await storeRepo.InsertAsync(store, autoSave: true);

            var customer = await customerRepo.FirstOrDefaultAsync(x => x.UserId == customerUserId);
            if (customer == null)
            {
                customer = new Customer(_guidGenerator.Create(), customerUserId, null, "ar");
                await customerRepo.InsertAsync(customer, autoSave: true);
            }

            var product = new Product(
                _guidGenerator.Create(),
                store.Id,
                "وجبة برجر مميز",
                "burger-" + Guid.NewGuid().ToString("N")[..8],
                1500M,
                "وجبة"
            );
            product.IsAvailable = true;
            await productRepo.InsertAsync(product, autoSave: true);

            var paymentMethod = await paymentMethodRepo.FirstOrDefaultAsync(x => x.Name == "CashOnDelivery");
            if (paymentMethod == null)
            {
                paymentMethod = new PaymentMethod(
                    _guidGenerator.Create(),
                    "CashOnDelivery",
                    "الدفع عند الاستلام",
                    isOnline: false,
                    requiresReceipt: false,
                    isActive: true
                );
                await paymentMethodRepo.InsertAsync(paymentMethod, autoSave: true);
            }

            var pendingStatus = await orderStatusRepo.FirstOrDefaultAsync(x => x.Name == "Pending");
            if (pendingStatus == null)
            {
                pendingStatus = new OrderStatus(
                    _guidGenerator.Create(),
                    "Pending",
                    "قيد الانتظار",
                    1,
                    false,
                    "#f39c12",
                    "fas fa-clock"
                );
                await orderStatusRepo.InsertAsync(pendingStatus, autoSave: true);
            }

            var acceptedStatus = await orderStatusRepo.FirstOrDefaultAsync(x => x.Name == "Accepted");
            if (acceptedStatus == null)
            {
                acceptedStatus = new OrderStatus(
                    _guidGenerator.Create(),
                    "Accepted",
                    "مقبول",
                    2,
                    false,
                    "#27ae60",
                    "fas fa-check"
                );
                await orderStatusRepo.InsertAsync(acceptedStatus, autoSave: true);
            }

            var rejectedStatus = await orderStatusRepo.FirstOrDefaultAsync(x => x.Name == "Rejected");
            if (rejectedStatus == null)
            {
                rejectedStatus = new OrderStatus(
                    _guidGenerator.Create(),
                    "Rejected",
                    "مرفوض",
                    3,
                    true,
                    "#c0392b",
                    "fas fa-times"
                );
                await orderStatusRepo.InsertAsync(rejectedStatus, autoSave: true);
            }

            var cancelledStatus = await orderStatusRepo.FirstOrDefaultAsync(x => x.Name == "Cancelled");
            if (cancelledStatus == null)
            {
                cancelledStatus = new OrderStatus(
                    _guidGenerator.Create(),
                    "Cancelled",
                    "ملغي",
                    4,
                    true,
                    "#7f8c8d",
                    "fas fa-ban"
                );
                await orderStatusRepo.InsertAsync(cancelledStatus, autoSave: true);
            }

            var deliveredStatus = await orderStatusRepo.FirstOrDefaultAsync(x => x.Name == "Delivered");
            if (deliveredStatus == null)
            {
                deliveredStatus = new OrderStatus(
                    _guidGenerator.Create(),
                    "Delivered",
                    "تم التسليم",
                    5,
                    true,
                    "#2ecc71",
                    "fas fa-box-check"
                );
                await orderStatusRepo.InsertAsync(deliveredStatus, autoSave: true);
            }

            return (store, customer, product, paymentMethod, pendingStatus);
        });
    }

    [Fact]
    public async Task CreateOrder_Should_Generate_Detailed_Notification_For_StoreOwner_And_Customer()
    {
        // Arrange
        var storeOwnerId = _guidGenerator.Create();
        var customerUserId = _guidGenerator.Create();
        var data = await SetupOrderPrerequisitesAsync(storeOwnerId, customerUserId);

        var createInput = new CreateOrderInput
        {
            StoreId = data.store.Id,
            PaymentMethodId = data.paymentMethod.Id,
            DeliveryAddressId = _guidGenerator.Create(),
            CustomerNotes = "يرجى الإسراع في التجهيز",
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = data.product.Id,
                    Quantity = 2
                }
            }
        };

        // Act: Create order as customer
        OrderDto createdOrder;
        using (ChangeUser(customerUserId, "customer_tester"))
        {
            createdOrder = await WithUowAsync(async sp =>
            {
                var orderAppService = sp.GetRequiredService<IOrderAppService>();
                return await orderAppService.PlaceOrderAsync(createInput);
            });
        }

        // Assert: Verify notifications in DB
        await WithUowAsync(async sp =>
        {
            var notifRepo = sp.GetRequiredService<IRepository<AppNotification, Guid>>();

            // 1. إشعار التاجر: يجب أن يكون وصله إشعار NewOrderReceived مع تفاصيل الطلب والمراجعة
            var storeOwnerNotifs = await notifRepo.GetListAsync(n => n.RecipientUserId == storeOwnerId);
            storeOwnerNotifs.Count.ShouldBeGreaterThanOrEqualTo(1);

            var storeNotif = storeOwnerNotifs.First();
            storeNotif.Title.ShouldContain("طلب جديد");
            storeNotif.Message.ShouldContain(createdOrder.OrderNumber);
            storeNotif.Message.ShouldContain("يرجى مراجعة");
            storeNotif.RelatedEntityId.ShouldBe(createdOrder.Id);

            // 2. إشعار العميل: تم استلام الطلب
            var customerNotifs = await notifRepo.GetListAsync(n => n.RecipientUserId == customerUserId);
            customerNotifs.Count.ShouldBeGreaterThanOrEqualTo(1);

            var customerNotif = customerNotifs.First();
            customerNotif.Title.ShouldContain("استلام");
            customerNotif.Message.ShouldContain(createdOrder.OrderNumber);
        });
    }

    [Fact]
    public async Task AcceptOrder_Should_Notify_Customer_With_Acceptance_And_Estimated_Time()
    {
        // Arrange
        var storeOwnerId = _guidGenerator.Create();
        var customerUserId = _guidGenerator.Create();
        var data = await SetupOrderPrerequisitesAsync(storeOwnerId, customerUserId);

        OrderDto order;
        using (ChangeUser(customerUserId, "customer_tester"))
        {
            order = await WithUowAsync(async sp =>
            {
                var orderAppService = sp.GetRequiredService<IOrderAppService>();
                return await orderAppService.PlaceOrderAsync(new CreateOrderInput
                {
                    StoreId = data.store.Id,
                    PaymentMethodId = data.paymentMethod.Id,
                    DeliveryAddressId = _guidGenerator.Create(),
                    Items = new List<CreateOrderItemDto>
                    {
                        new CreateOrderItemDto { ProductId = data.product.Id, Quantity = 1 }
                    }
                });
            });
        }

        // Act: Store owner accepts order
        using (ChangeUser(storeOwnerId, "merchant_tester"))
        {
            await WithUowAsync(async sp =>
            {
                var orderAppService = sp.GetRequiredService<IOrderAppService>();
                await orderAppService.AcceptOrderAsync(order.Id, new AcceptOrderInput
                {
                    EstimatedMinutes = 30,
                    Notes = "تم بدء تحضير الوجبة"
                });
            });
        }

        // Assert: Check notification to customer
        await WithUowAsync(async sp =>
        {
            var notifRepo = sp.GetRequiredService<IRepository<AppNotification, Guid>>();
            var notifs = await notifRepo.GetListAsync(n => n.RecipientUserId == customerUserId && n.Title.Contains("قبول"));
            notifs.Count.ShouldBe(1);

            var notif = notifs.First();
            notif.Message.ShouldContain(order.OrderNumber);
            notif.Message.ShouldContain(data.store.Name);
            notif.Message.ShouldContain("تحضيره وتجهيزه");
        });
    }

    [Fact]
    public async Task RejectOrder_Should_Notify_Customer_With_Specific_Rejection_Reason()
    {
        // Arrange
        var storeOwnerId = _guidGenerator.Create();
        var customerUserId = _guidGenerator.Create();
        var data = await SetupOrderPrerequisitesAsync(storeOwnerId, customerUserId);

        OrderDto order;
        using (ChangeUser(customerUserId, "customer_tester"))
        {
            order = await WithUowAsync(async sp =>
            {
                var orderAppService = sp.GetRequiredService<IOrderAppService>();
                return await orderAppService.PlaceOrderAsync(new CreateOrderInput
                {
                    StoreId = data.store.Id,
                    PaymentMethodId = data.paymentMethod.Id,
                    DeliveryAddressId = _guidGenerator.Create(),
                    Items = new List<CreateOrderItemDto>
                    {
                        new CreateOrderItemDto { ProductId = data.product.Id, Quantity = 1 }
                    }
                });
            });
        }

        // Act: Store owner rejects order with custom reason
        const string rejectionReason = "نفاد الكمية المطلوبة من المطبخ حالياً";
        using (ChangeUser(storeOwnerId, "merchant_tester"))
        {
            await WithUowAsync(async sp =>
            {
                var reasonRepo = sp.GetRequiredService<IRepository<CancellationReason, Guid>>();
                var reason = await reasonRepo.FirstOrDefaultAsync(x => x.TargetAudience == CancellationTargetAudience.Store);
                if (reason == null)
                {
                    reason = new CancellationReason(_guidGenerator.Create(), "نفاد الكمية", CancellationTargetAudience.Store, true);
                    await reasonRepo.InsertAsync(reason, autoSave: true);
                }

                var orderAppService = sp.GetRequiredService<IOrderAppService>();
                await orderAppService.RejectOrderAsync(order.Id, new RejectOrderInput
                {
                    OrderId = order.Id,
                    RejectionReasonId = reason.Id,
                    AdditionalNotes = rejectionReason
                });
            });
        }

        // Assert: Check rejection notification sent to customer
        await WithUowAsync(async sp =>
        {
            var notifRepo = sp.GetRequiredService<IRepository<AppNotification, Guid>>();
            var notifs = await notifRepo.GetListAsync(n => n.RecipientUserId == customerUserId && n.Title.Contains("رفض"));
            notifs.Count.ShouldBe(1);

            var notif = notifs.First();
            notif.Message.ShouldContain(order.OrderNumber);
            notif.Message.ShouldContain(rejectionReason);
            notif.Message.ShouldContain(data.store.Name);
        });
    }

    [Fact]
    public async Task CancelOrder_Should_Notify_Store_With_Cancellation_Reason()
    {
        // Arrange
        var storeOwnerId = _guidGenerator.Create();
        var customerUserId = _guidGenerator.Create();
        var data = await SetupOrderPrerequisitesAsync(storeOwnerId, customerUserId);

        OrderDto order;
        using (ChangeUser(customerUserId, "customer_tester"))
        {
            order = await WithUowAsync(async sp =>
            {
                var orderAppService = sp.GetRequiredService<IOrderAppService>();
                return await orderAppService.PlaceOrderAsync(new CreateOrderInput
                {
                    StoreId = data.store.Id,
                    PaymentMethodId = data.paymentMethod.Id,
                    DeliveryAddressId = _guidGenerator.Create(),
                    Items = new List<CreateOrderItemDto>
                    {
                        new CreateOrderItemDto { ProductId = data.product.Id, Quantity = 1 }
                    }
                });
            });
        }

        // Act: Customer cancels the order
        const string cancellationReason = "تغيير في خطة العشاء";
        using (ChangeUser(customerUserId, "customer_tester"))
        {
            await WithUowAsync(async sp =>
            {
                var orderAppService = sp.GetRequiredService<IOrderAppService>();
                await orderAppService.CancelOrderAsync(order.Id, new CancelOrderInput
                {
                    AdditionalNotes = cancellationReason
                });
            });
        }

        // Assert: Check store owner notification
        await WithUowAsync(async sp =>
        {
            var notifRepo = sp.GetRequiredService<IRepository<AppNotification, Guid>>();
            var notifs = await notifRepo.GetListAsync(n => n.RecipientUserId == storeOwnerId && n.Title.Contains("إلغاء"));
            notifs.Count.ShouldBe(1);

            var notif = notifs.First();
            notif.Message.ShouldContain(order.OrderNumber);
            notif.Message.ShouldContain(cancellationReason);
        });
    }

    [Fact]
    public async Task DeliveryLifecycle_Should_Send_Notifications_To_Correct_User_Ids()
    {
        // Arrange
        var storeOwnerId = _guidGenerator.Create();
        var customerUserId = _guidGenerator.Create();
        var courierUserId = _guidGenerator.Create();
        var data = await SetupOrderPrerequisitesAsync(storeOwnerId, customerUserId);

        // Setup Courier
        Courier courier = null!;
        await WithUowAsync(async sp =>
        {
            var courierRepo = sp.GetRequiredService<IRepository<Courier, Guid>>();
            var userRepo = sp.GetRequiredService<IRepository<IdentityUser, Guid>>();

            var uniqueSuffix = Guid.NewGuid().ToString("N")[..6];
            var courierUser = new IdentityUser(courierUserId, "courier_" + uniqueSuffix, $"courier_{uniqueSuffix}@talabi.com");
            courierUser.SetPhoneNumber("777888999", true);
            await userRepo.InsertAsync(courierUser, autoSave: true);

            courier = new Courier(
                _guidGenerator.Create(),
                courierUserId,
                "دراجة نارية",
                "123-صنعاء"
            );
            courier.IsAvailable = true;
            courier.IsOnline = true;
            await courierRepo.InsertAsync(courier, autoSave: true);
        });

        // Create and accept order
        OrderDto order;
        using (ChangeUser(customerUserId, "customer_tester"))
        {
            order = await WithUowAsync(async sp =>
            {
                var orderAppService = sp.GetRequiredService<IOrderAppService>();
                return await orderAppService.PlaceOrderAsync(new CreateOrderInput
                {
                    StoreId = data.store.Id,
                    PaymentMethodId = data.paymentMethod.Id,
                    DeliveryAddressId = _guidGenerator.Create(),
                    Items = new List<CreateOrderItemDto>
                    {
                        new CreateOrderItemDto { ProductId = data.product.Id, Quantity = 1 }
                    }
                });
            });
        }

        // Step 1: Assign courier
        DeliveryAssignmentDto assignment;
        using (ChangeUser(storeOwnerId, "merchant_tester"))
        {
            assignment = await WithUowAsync(async sp =>
            {
                var deliveryAppService = sp.GetRequiredService<IDeliveryAppService>();
                return await deliveryAppService.AssignCourierAsync(new AssignCourierInput
                {
                    OrderId = order.Id,
                    CourierId = courier.Id
                });
            });
        }

        // Assert Step 1: Courier received assignment notification
        await WithUowAsync(async sp =>
        {
            var notifRepo = sp.GetRequiredService<IRepository<AppNotification, Guid>>();
            var courierNotifs = await notifRepo.GetListAsync(n => n.RecipientUserId == courierUserId);
            courierNotifs.Count.ShouldBeGreaterThanOrEqualTo(1);
            courierNotifs.Any(n => n.Title.Contains("توصيل")).ShouldBeTrue();
        });

        // Step 2: Courier accepts delivery
        using (ChangeUser(courierUserId, "courier_ahmed"))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryAppService = sp.GetRequiredService<IDeliveryAppService>();
                await deliveryAppService.AcceptDeliveryAsync(assignment.Id);
            });
        }

        // Step 3: Courier picks up delivery (out for delivery)
        using (ChangeUser(courierUserId, "courier_ahmed"))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryAppService = sp.GetRequiredService<IDeliveryAppService>();
                await deliveryAppService.PickupDeliveryAsync(assignment.Id);
            });
        }

        // Assert Step 3: Customer received "OutForDelivery" notification using Customer.UserId (NOT Customer.Id)
        await WithUowAsync(async sp =>
        {
            var notifRepo = sp.GetRequiredService<IRepository<AppNotification, Guid>>();
            var outForDeliveryNotifs = await notifRepo.GetListAsync(n => n.RecipientUserId == customerUserId && n.Title.Contains("الطريق"));
            outForDeliveryNotifs.Count.ShouldBe(1);
            outForDeliveryNotifs.First().Message.ShouldContain(order.OrderNumber);
        });

        // Step 4: Confirm delivery
        using (ChangeUser(courierUserId, "courier_ahmed"))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryAppService = sp.GetRequiredService<IDeliveryAppService>();
                await deliveryAppService.ConfirmDeliveryAsync(new ConfirmDeliveryInput
                {
                    DeliveryAssignmentId = assignment.Id,
                    ConfirmedBy = DeliveryConfirmedBy.Courier,
                    Notes = "تم التسليم للعميل يداً بيد"
                });
            });
        }

        // Assert Step 4: Customer received "OrderDelivered" notification with rating invite
        await WithUowAsync(async sp =>
        {
            var notifRepo = sp.GetRequiredService<IRepository<AppNotification, Guid>>();
            var deliveredNotifs = await notifRepo.GetListAsync(n => n.RecipientUserId == customerUserId && n.Title.Contains("تسليم"));
            deliveredNotifs.Count.ShouldBe(1);
            deliveredNotifs.First().Message.ShouldContain("تقييمك للمتجر");

            // Also store owner received completion notification
            var storeDeliveredNotifs = await notifRepo.GetListAsync(n => n.RecipientUserId == storeOwnerId && n.Title.Contains("تسليم"));
            storeDeliveredNotifs.Count.ShouldBe(1);
        });
    }
}
