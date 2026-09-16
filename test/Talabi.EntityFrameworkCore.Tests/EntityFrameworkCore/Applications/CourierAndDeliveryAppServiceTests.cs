using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Talabi.Customers;
using Talabi.Deliveries;
using Talabi.Deliveries.Dtos;
using Talabi.Orders;
using Talabi.Payments;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace Talabi.EntityFrameworkCore.Applications;

/// <summary>
/// اختبارات وحدة وتكامل خدمتي إدارة المناديب وعمليات التوصيل
/// CourierAppService & DeliveryAppService
/// </summary>
[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class CourierAndDeliveryAppServiceTests : TalabiEntityFrameworkCoreTestBase
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public CourierAndDeliveryAppServiceTests()
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

    private IDisposable ChangeUser(Guid userId, string? userName = "test_user", string? role = null)
    {
        var claims = new System.Collections.Generic.List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, userName ?? "test_user")
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(AbpClaimTypes.Role, role));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        return _currentPrincipalAccessor.Change(principal);
    }

    private async Task<(IdentityUser user, Courier courier)> CreateTestCourierUserAsync()
    {
        return await WithUowAsync(async sp =>
        {
            var userRepo = sp.GetRequiredService<IRepository<IdentityUser, Guid>>();
            var courierRepo = sp.GetRequiredService<IRepository<Courier, Guid>>();

            var user = new IdentityUser(
                _guidGenerator.Create(),
                "driver_" + Guid.NewGuid().ToString("N")[..6],
                "driver_" + Guid.NewGuid().ToString("N")[..6] + "@talabi.com"
            );
            await userRepo.InsertAsync(user, autoSave: true);

            var courier = new Courier(
                _guidGenerator.Create(),
                user.Id,
                "دراجة نارية",
                "1234-صنعاء"
            );
            courier.LicenseNumber = "LIC-9988";
            courier.IsAvailable = true;
            courier.IsOnline = true;
            await courierRepo.InsertAsync(courier, autoSave: true);

            return (user, courier);
        });
    }

    private async Task<Order> CreateTestOrderAsync()
    {
        return await WithUowAsync(async sp =>
        {
            var storeTypeRepo = sp.GetRequiredService<IRepository<StoreType, Guid>>();
            var storeRepo = sp.GetRequiredService<IRepository<Store, Guid>>();
            var customerRepo = sp.GetRequiredService<IRepository<Customer, Guid>>();
            var statusRepo = sp.GetRequiredService<IRepository<OrderStatus, Guid>>();
            var paymentMethodRepo = sp.GetRequiredService<IRepository<PaymentMethod, Guid>>();
            var orderRepo = sp.GetRequiredService<IRepository<Order, Guid>>();

            var storeType = new StoreType(_guidGenerator.Create(), "مطاعم", 1, true);
            await storeTypeRepo.InsertAsync(storeType, autoSave: true);

            var store = new Store(
                _guidGenerator.Create(),
                _guidGenerator.Create(),
                storeType.Id,
                "متجر الاختبار",
                "store-" + Guid.NewGuid().ToString("N")[..6],
                "777000000",
                "صنعاء",
                15.35M,
                44.20M
            );
            await storeRepo.InsertAsync(store, autoSave: true);

            var customer = new Customer(_guidGenerator.Create(), _guidGenerator.Create(), null, "ar");
            await customerRepo.InsertAsync(customer, autoSave: true);

            var pendingStatus = await statusRepo.FirstOrDefaultAsync(x => x.Name == "Pending");
            if (pendingStatus == null)
            {
                pendingStatus = new OrderStatus(_guidGenerator.Create(), "Pending", "قيد الانتظار", 1);
                await statusRepo.InsertAsync(pendingStatus, autoSave: true);
            }

            var outForDeliveryStatus = await statusRepo.FirstOrDefaultAsync(x => x.Name == "OutForDelivery");
            if (outForDeliveryStatus == null)
            {
                outForDeliveryStatus = new OrderStatus(_guidGenerator.Create(), "OutForDelivery", "في الطريق", 4);
                await statusRepo.InsertAsync(outForDeliveryStatus, autoSave: true);
            }

            var deliveredStatus = await statusRepo.FirstOrDefaultAsync(x => x.Name == "Delivered");
            if (deliveredStatus == null)
            {
                deliveredStatus = new OrderStatus(_guidGenerator.Create(), "Delivered", "تم التسليم", 5);
                await statusRepo.InsertAsync(deliveredStatus, autoSave: true);
            }

            var paymentMethod = await paymentMethodRepo.FirstOrDefaultAsync(x => x.Name == "CashOnDelivery");
            if (paymentMethod == null)
            {
                paymentMethod = new PaymentMethod(_guidGenerator.Create(), "CashOnDelivery", "الدفع عند الاستلام", false, false, true);
                await paymentMethodRepo.InsertAsync(paymentMethod, autoSave: true);
            }

            var order = new Order(
                _guidGenerator.Create(),
                "ORD-" + Random.Shared.Next(1000, 9999),
                customer.Id,
                store.Id,
                _guidGenerator.Create(),
                "صنعاء - شارع حدة",
                pendingStatus.Id,
                paymentMethod.Id,
                5000,
                5500
            );
            await orderRepo.InsertAsync(order, autoSave: true);

            return order;
        });
    }

    [Fact]
    public async Task CreateAsync_Should_Register_New_Courier_Successfully()
    {
        // Arrange
        var testUser = await WithUowAsync(async sp =>
        {
            var userRepo = sp.GetRequiredService<IRepository<IdentityUser, Guid>>();
            var user = new IdentityUser(_guidGenerator.Create(), "courier_candidate", "cand@talabi.com");
            await userRepo.InsertAsync(user, autoSave: true);
            return user;
        });

        var input = new CreateCourierDto
        {
            UserId = testUser.Id,
            VehicleType = "سيارة",
            VehicleNumber = "9988-صنعاء",
            LicenseNumber = "DL-12345"
        };

        // Act
        CourierDto result = null!;
        using (ChangeUser(Guid.NewGuid(), "admin", "admin"))
        {
            await WithUowAsync(async sp =>
            {
                var appService = sp.GetRequiredService<ICourierAppService>();
                result = await appService.CreateAsync(input);
            });
        }

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(testUser.Id);
        result.VehicleType.ShouldBe("سيارة");
        result.VehicleNumber.ShouldBe("9988-صنعاء");
        result.LicenseNumber.ShouldBe("DL-12345");
        result.IsAvailable.ShouldBeTrue();
        result.IsOnline.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateLocationAsync_And_SetAvailability_Should_Update_Courier_State()
    {
        // Arrange
        var (user, courier) = await CreateTestCourierUserAsync();

        // Act
        CourierDto updatedCourier = null!;
        using (ChangeUser(user.Id))
        {
            await WithUowAsync(async sp =>
            {
                var appService = sp.GetRequiredService<ICourierAppService>();

                await appService.SetAvailabilityAsync(false);
                await appService.SetOnlineStatusAsync(true);

                updatedCourier = await appService.UpdateLocationAsync(new UpdateCourierLocationInput
                {
                    Latitude = 15.369444M,
                    Longitude = 44.191000M
                });
            });
        }

        // Assert
        updatedCourier.ShouldNotBeNull();
        updatedCourier.IsAvailable.ShouldBeFalse();
        updatedCourier.IsOnline.ShouldBeTrue();
        updatedCourier.CurrentLatitude.ShouldBe(15.369444M);
        updatedCourier.CurrentLongitude.ShouldBe(44.191000M);
        updatedCourier.LastLocationUpdate.ShouldNotBeNull();
    }

    [Fact]
    public async Task AssignCourierAsync_Should_Create_Assignment_And_Set_Courier_Busy()
    {
        // Arrange
        var (courierUser, courier) = await CreateTestCourierUserAsync();
        var order = await CreateTestOrderAsync();

        // Act
        DeliveryAssignmentDto assignmentResult = null!;
        using (ChangeUser(Guid.NewGuid(), "store_owner", "admin"))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryService = sp.GetRequiredService<IDeliveryAppService>();
                assignmentResult = await deliveryService.AssignCourierAsync(new AssignCourierInput
                {
                    OrderId = order.Id,
                    CourierId = courier.Id
                });
            });
        }

        // Assert
        assignmentResult.ShouldNotBeNull();
        assignmentResult.OrderId.ShouldBe(order.Id);
        assignmentResult.CourierId.ShouldBe(courier.Id);
        assignmentResult.Status.ShouldBe(DeliveryAssignmentStatus.Assigned);

        // التحقق من أن المندوب أصبح غير متاح
        await WithUowAsync(async sp =>
        {
            var courierRepo = sp.GetRequiredService<IRepository<Courier, Guid>>();
            var updatedCourier = await courierRepo.GetAsync(courier.Id);
            updatedCourier.IsAvailable.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task AcceptDelivery_And_PickupDelivery_Should_Progress_Assignment_Stages()
    {
        // Arrange
        var (courierUser, courier) = await CreateTestCourierUserAsync();
        var order = await CreateTestOrderAsync();

        DeliveryAssignmentDto assignment = null!;
        using (ChangeUser(Guid.NewGuid(), "admin", "admin"))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryService = sp.GetRequiredService<IDeliveryAppService>();
                assignment = await deliveryService.AssignCourierAsync(new AssignCourierInput
                {
                    OrderId = order.Id,
                    CourierId = courier.Id
                });
            });
        }

        // Act & Assert 1: قبول الطلب
        using (ChangeUser(courierUser.Id))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryService = sp.GetRequiredService<IDeliveryAppService>();
                var accepted = await deliveryService.AcceptDeliveryAsync(assignment.Id);
                accepted.Status.ShouldBe(DeliveryAssignmentStatus.Accepted);
                accepted.AcceptedAt.ShouldNotBeNull();
            });
        }

        // Act & Assert 2: استلام الطلب من المتجر
        using (ChangeUser(courierUser.Id))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryService = sp.GetRequiredService<IDeliveryAppService>();
                var pickedUp = await deliveryService.PickupDeliveryAsync(assignment.Id);
                pickedUp.Status.ShouldBe(DeliveryAssignmentStatus.PickedUp);
                pickedUp.PickupAt.ShouldNotBeNull();
            });
        }

        // التحقق من انتقال حالة الطلب إلى في الطريق OutForDelivery
        await WithUowAsync(async sp =>
        {
            var orderRepo = sp.GetRequiredService<IRepository<Order, Guid>>();
            var statusRepo = sp.GetRequiredService<IRepository<OrderStatus, Guid>>();
            var updatedOrder = await orderRepo.GetAsync(order.Id);
            var currentStatus = await statusRepo.GetAsync(updatedOrder.OrderStatusId);
            currentStatus.Name.ShouldBeOneOf("OutForDelivery", "InTransit");
        });
    }

    [Fact]
    public async Task ConfirmDeliveryAsync_Should_Complete_Delivery_And_Release_Courier()
    {
        // Arrange
        var (courierUser, courier) = await CreateTestCourierUserAsync();
        var order = await CreateTestOrderAsync();

        DeliveryAssignmentDto assignment = null!;
        using (ChangeUser(Guid.NewGuid(), "admin", "admin"))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryService = sp.GetRequiredService<IDeliveryAppService>();
                assignment = await deliveryService.AssignCourierAsync(new AssignCourierInput
                {
                    OrderId = order.Id,
                    CourierId = courier.Id
                });
            });
        }

        using (ChangeUser(courierUser.Id))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryService = sp.GetRequiredService<IDeliveryAppService>();
                await deliveryService.AcceptDeliveryAsync(assignment.Id);
                await deliveryService.PickupDeliveryAsync(assignment.Id);
            });
        }

        // Act: تأكيد تسليم الطلب للعميل
        DeliveryAssignmentDto confirmedResult = null!;
        using (ChangeUser(courierUser.Id))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryService = sp.GetRequiredService<IDeliveryAppService>();
                confirmedResult = await deliveryService.ConfirmDeliveryAsync(new ConfirmDeliveryInput
                {
                    DeliveryAssignmentId = assignment.Id,
                    ConfirmedBy = DeliveryConfirmedBy.Courier,
                    VerificationCode = "5544",
                    Notes = "تم التسليم للعميل يداً بيد"
                });
            });
        }

        // Assert
        confirmedResult.ShouldNotBeNull();
        confirmedResult.Status.ShouldBe(DeliveryAssignmentStatus.Delivered);
        confirmedResult.DeliveredAt.ShouldNotBeNull();
        confirmedResult.Confirmation.ShouldNotBeNull();
        confirmedResult.Confirmation!.VerificationCode.ShouldBe("5544");
        confirmedResult.Confirmation.Notes.ShouldBe("تم التسليم للعميل يداً بيد");

        // التحقق من إتاحة المندوب وزيادة إجمالي التوصيلات
        await WithUowAsync(async sp =>
        {
            var courierRepo = sp.GetRequiredService<IRepository<Courier, Guid>>();
            var updatedCourier = await courierRepo.GetAsync(courier.Id);
            updatedCourier.IsAvailable.ShouldBeTrue();
            updatedCourier.TotalDeliveries.ShouldBe(1);
        });

        // التحقق من اكتمال حالة الطلب إلى Delivered
        await WithUowAsync(async sp =>
        {
            var orderRepo = sp.GetRequiredService<IRepository<Order, Guid>>();
            var statusRepo = sp.GetRequiredService<IRepository<OrderStatus, Guid>>();
            var updatedOrder = await orderRepo.GetAsync(order.Id);
            var currentStatus = await statusRepo.GetAsync(updatedOrder.OrderStatusId);
            currentStatus.Name.ShouldBe("Delivered");
        });
    }

    [Fact]
    public async Task FailDeliveryAsync_Should_Mark_Assignment_Failed_And_Release_Courier()
    {
        // Arrange
        var (courierUser, courier) = await CreateTestCourierUserAsync();
        var order = await CreateTestOrderAsync();

        DeliveryAssignmentDto assignment = null!;
        using (ChangeUser(Guid.NewGuid(), "admin", "admin"))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryService = sp.GetRequiredService<IDeliveryAppService>();
                assignment = await deliveryService.AssignCourierAsync(new AssignCourierInput
                {
                    OrderId = order.Id,
                    CourierId = courier.Id
                });
            });
        }

        // Act: تعثر التوصيل
        DeliveryAssignmentDto failedResult = null!;
        using (ChangeUser(courierUser.Id))
        {
            await WithUowAsync(async sp =>
            {
                var deliveryService = sp.GetRequiredService<IDeliveryAppService>();
                failedResult = await deliveryService.FailDeliveryAsync(assignment.Id, new FailDeliveryInput
                {
                    Reason = "العميل مغلق هاتفه وعنوانه غير دقيق"
                });
            });
        }

        // Assert
        failedResult.ShouldNotBeNull();
        failedResult.Status.ShouldBe(DeliveryAssignmentStatus.Failed);

        // المندوب يعود متاحاً
        await WithUowAsync(async sp =>
        {
            var courierRepo = sp.GetRequiredService<IRepository<Courier, Guid>>();
            var updatedCourier = await courierRepo.GetAsync(courier.Id);
            updatedCourier.IsAvailable.ShouldBeTrue();
        });
    }
}
