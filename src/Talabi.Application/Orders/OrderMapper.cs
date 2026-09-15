using Riok.Mapperly.Abstractions;
using Talabi.Orders.Dtos;

namespace Talabi.Orders;

[Mapper(UseDeepCloning = true)]
public partial class OrderMapper
{
    [MapProperty("Store.Name", nameof(OrderDto.StoreName))]
    [MapProperty("OrderStatus.DisplayName", nameof(OrderDto.OrderStatusName))]
    [MapProperty("OrderStatus.Color", nameof(OrderDto.OrderStatusColor))]
    public partial OrderDto MapToOrderDto(Order order);

    public partial OrderItemDto MapToOrderItemDto(OrderItem orderItem);
}
