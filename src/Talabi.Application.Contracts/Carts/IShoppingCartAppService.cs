using System;
using System.Threading.Tasks;
using Talabi.Carts.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Carts;

/// <summary>
/// واجهة خدمة سلة مشتريات العميل الحالي
/// </summary>
public interface IShoppingCartAppService : IApplicationService
{
    Task<ShoppingCartDto> GetMyCartAsync();

    Task<ShoppingCartDto> AddItemAsync(AddShoppingCartItemInput input);

    Task<ShoppingCartDto> UpdateItemQuantityAsync(Guid itemId, UpdateShoppingCartItemQuantityInput input);

    Task<ShoppingCartDto> RemoveItemAsync(Guid itemId);

    Task ClearCartAsync();
}
