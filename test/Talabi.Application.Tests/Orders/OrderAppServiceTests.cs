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

namespace Talabi.Orders;

public class OrderAppServiceTests : TalabiApplicationTestBase<TalabiApplicationTestModule>
{
    private readonly IOrderAppService _orderAppService;
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<PaymentMethod, Guid> _paymentMethodRepository;
    private readonly IGuidGenerator _guidGenerator;

    public OrderAppServiceTests()
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
        // 1. Arrange: Seed required data
        var storeId = _guidGenerator.Create();
        var store = new Store(
            storeId,
            Guid.Empty,
            Guid.Empty,
            "Test Store",
            "test-store",
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
        await _storeRepository.InsertAsync(store, autoSave: true);

        var productId = _guidGenerator.Create();
        var product = new Product(
            productId,
            storeId,
            "Test Product",
            "SKU-123",
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
        await _productRepository.InsertAsync(product, autoSave: true);

        var paymentMethod = new PaymentMethod(
            _guidGenerator.Create(),
            "Cash on Delivery",
            "COD",
            false,
            false,
            true,
            null
        );
        // We might need to resolve standard repository if PaymentMethodRepository doesn't exist
        var standardPaymentRepo = GetRequiredService<IRepository<PaymentMethod, Guid>>();
        await standardPaymentRepo.InsertAsync(paymentMethod, autoSave: true);

        // 2. Act: Place Order
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

        var placedOrder = await _orderAppService.PlaceOrderAsync(input);

        // 3. Assert Place Order
        placedOrder.ShouldNotBeNull();
        placedOrder.Id.ShouldNotBe(Guid.Empty);
        placedOrder.StoreId.ShouldBe(storeId);
        placedOrder.Items.Count.ShouldBe(1);
        placedOrder.Items.First().ProductId.ShouldBe(productId);
        placedOrder.SubTotal.ShouldBe(200); // 100 * 2
        placedOrder.OrderStatusName.ShouldBe("Pending");

        // 4. Act: Accept Order
        var acceptedOrder = await _orderAppService.AcceptOrderAsync(placedOrder.Id, "We received your order");
        acceptedOrder.OrderStatusName.ShouldBeOneOf("Accepted", "Preparing");

        // 5. Act: Assign Driver
        var driverId = _guidGenerator.Create();
        var assignedOrder = await _orderAppService.AssignDriverAsync(placedOrder.Id, driverId);
        assignedOrder.CourierId.ShouldBe(driverId);

        // 6. Act: Complete Order
        var completedOrder = await _orderAppService.CompleteOrderAsync(placedOrder.Id, "Delivered successfully");
        completedOrder.OrderStatusName.ShouldBe("Delivered");
        completedOrder.ActualDeliveryTime.ShouldNotBeNull();
    }
}
