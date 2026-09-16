using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Talabi.Orders;
using Talabi.Orders.Dtos;
using Talabi.Payments;
using Talabi.Products;
using Talabi.Stores;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

using Talabi.Customers;
using Talabi.Deliveries;
using Volo.Abp.Modularity;

namespace Talabi.Orders;

public abstract class OrderAppServiceTests<TStartupModule> : TalabiApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IOrderAppService _orderAppService;
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<PaymentMethod, Guid> _paymentMethodRepository;
    private readonly IGuidGenerator _guidGenerator;

    protected OrderAppServiceTests()
    {
        _orderAppService = GetRequiredService<IOrderAppService>();
        _storeRepository = GetRequiredService<IRepository<Store, Guid>>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        _paymentMethodRepository = GetRequiredService<IRepository<PaymentMethod, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Should_Place_And_Complete_Order_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // 1. Arrange: Seed required data
            var currentUser = GetRequiredService<Volo.Abp.Users.ICurrentUser>();
            var currentUserId = currentUser.Id ?? _guidGenerator.Create();
            var customerRepo = GetRequiredService<IRepository<Customer, Guid>>();
            var customer = await customerRepo.FirstOrDefaultAsync(x => x.UserId == currentUserId);
            if (customer == null)
            {
                customer = new Customer(_guidGenerator.Create(), currentUserId, null, "ar");
                await customerRepo.InsertAsync(customer, autoSave: true);
            }

            var storeTypeRepo = GetRequiredService<IRepository<StoreType, Guid>>();
            var storeType = await storeTypeRepo.FirstOrDefaultAsync();
            if (storeType == null)
            {
                storeType = new StoreType(_guidGenerator.Create(), "مطاعم", 1, true);
                await storeTypeRepo.InsertAsync(storeType, autoSave: true);
            }

            var storeRepo = GetRequiredService<IRepository<Store, Guid>>();
            var storeId = _guidGenerator.Create();
            var store = new Store(
                storeId,
                _guidGenerator.Create(),
                storeType.Id,
                "Test Store " + Guid.NewGuid().ToString("N")[..4],
                "test-store-" + Guid.NewGuid().ToString("N")[..8],
                "123456789",
                "Address",
                0,
                0,
                null
            )
            {
                IsActive = true,
                Status = StoreStatus.Active
            };
            await storeRepo.InsertAsync(store, autoSave: true);

            var productRepo = GetRequiredService<IRepository<Product, Guid>>();
            var productId = _guidGenerator.Create();
            var product = new Product(
                productId,
                storeId,
                "Test Product",
                "SKU-" + Guid.NewGuid().ToString("N")[..6],
                100,
                "Piece",
                null,
                0,
                DiscountType.Fixed,
                null
            )
            {
                IsActive = true,
                IsAvailable = true
            };
            await productRepo.InsertAsync(product, autoSave: true);

            var standardPaymentRepo = GetRequiredService<IRepository<PaymentMethod, Guid>>();
            var paymentMethod = await standardPaymentRepo.FirstOrDefaultAsync(x => x.Name == "Cash on Delivery");
            if (paymentMethod == null)
            {
                paymentMethod = new PaymentMethod(
                    _guidGenerator.Create(),
                    "Cash on Delivery",
                    "COD",
                    false,
                    false,
                    true,
                    null
                );
                await standardPaymentRepo.InsertAsync(paymentMethod, autoSave: true);
            }

            // 2. Act: Place Order
            var orderAppService = GetRequiredService<IOrderAppService>();
            var input = new CreateOrderInput
            {
                StoreId = storeId,
                PaymentMethodId = paymentMethod.Id,
                DeliveryAddressId = _guidGenerator.Create(),
                CustomerNotes = "Please deliver fast",
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto
                    {
                        ProductId = productId,
                        Quantity = 2,
                        Notes = "No sugar"
                    }
                }
            };

            var placedOrder = await orderAppService.PlaceOrderAsync(input);

            // 3. Assert Place Order
            placedOrder.ShouldNotBeNull();
            placedOrder.Id.ShouldNotBe(Guid.Empty);
            placedOrder.StoreId.ShouldBe(storeId);
            placedOrder.Items.Count.ShouldBe(1);
            placedOrder.Items.First().ProductId.ShouldBe(productId);
            placedOrder.SubTotal.ShouldBe(200); // 100 * 2
            placedOrder.OrderStatusName.ShouldBeOneOf("Pending", "قيد الانتظار");

            // 4. Act: Accept Order
            var acceptedOrder = await orderAppService.AcceptOrderAsync(placedOrder.Id, new Talabi.Orders.Dtos.AcceptOrderInput { Notes = "We received your order" });
            acceptedOrder.OrderStatusName.ShouldBeOneOf("Accepted", "Preparing", "مقبول", "قيد التجهيز", "تم القبول");

            // 5. Act: Assign Driver
            var courierRepo = GetRequiredService<IRepository<Courier, Guid>>();
            var driverId = _guidGenerator.Create();
            var courier = new Courier(
                driverId,
                _guidGenerator.Create(),
                "دراجة",
                "777-صنعاء"
            );
            courier.IsAvailable = true;
            await courierRepo.InsertAsync(courier, autoSave: true);

            var assignedOrder = await orderAppService.AssignDriverAsync(placedOrder.Id, driverId);
            assignedOrder.CourierId.ShouldBe(driverId);

            var completedOrder = await orderAppService.CompleteOrderAsync(placedOrder.Id, "Delivered successfully");
            completedOrder.OrderStatusName.ShouldBeOneOf("Delivered", "تم التسليم", "تم التوصيل");
            completedOrder.ActualDeliveryTime.ShouldNotBeNull();
        });
    }
}
