using Riok.Mapperly.Abstractions;
using Talabi.Carts.Dtos;

namespace Talabi.Carts;

/// <summary>
/// إعدادات Mapperly الخاصة بسلة المشتريات
/// </summary>
[Mapper]
public partial class ShoppingCartMapper
{
    [MapProperty(nameof(Cart.Id), nameof(ShoppingCartDto.Id))]
    public partial ShoppingCartDto ToShoppingCartDto(Cart source);

    [MapperIgnoreSource(nameof(CartItem.Cart))]
    [MapperIgnoreSource(nameof(CartItem.Product))]
    [MapProperty(nameof(CartItem.UnitPriceAtAddition), nameof(ShoppingCartItemDto.UnitPrice))]
    public partial ShoppingCartItemDto ToShoppingCartItemDto(CartItem source);
}
