using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Talabi.Carts.Dtos;
using Talabi.Customers;
using Talabi.Products;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Talabi.Carts;

/// <summary>
/// خدمة تطبيق سلة مشتريات العميل الحالي
/// </summary>
[Authorize]
public class ShoppingCartAppService : ApplicationService, IShoppingCartAppService
{
    #region 1. Fields & Dependencies

    private readonly IRepository<Cart, Guid> _cartRepository;
    private readonly IRepository<CartItem, Guid> _cartItemRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly ShoppingCartMapper _mapper;

    #endregion

    #region 2. Constructors

    public ShoppingCartAppService(
        IRepository<Cart, Guid> cartRepository,
        IRepository<CartItem, Guid> cartItemRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<Store, Guid> storeRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _storeRepository = storeRepository;
        _mapper = new ShoppingCartMapper();
    }

    #endregion

    #region 3. Public Methods / Actions

    public async Task<ShoppingCartDto> GetMyCartAsync()
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await FindActiveCartAsync(customer.Id);

        if (cart is null)
        {
            return new ShoppingCartDto
            {
                CustomerId = customer.Id
            };
        }

        return await BuildCartDtoAsync(cart, customer.Id, updateStoredTotals: true);
    }

    public async Task<ShoppingCartDto> AddItemAsync(AddShoppingCartItemInput input)
    {
        if (input.Quantity <= 0)
        {
            throw new UserFriendlyException("يجب أن تكون الكمية 1 على الأقل. لا يمكن إضافة كمية صفر أو سالبة.");
        }

        var customer = await GetCurrentCustomerAsync();
        var productQuery = await _productRepository.WithDetailsAsync(x => x.SalesUnits);
        var product = await AsyncExecuter.FirstOrDefaultAsync(productQuery.Where(x => x.Id == input.ProductId));
        if (product == null)
        {
            throw new EntityNotFoundException(typeof(Product), input.ProductId);
        }

        EnsureProductCanBeOrdered(product);
        ValidateQuantity(product, input.Quantity);

        // تحديد وحدة البيع والسعر:
        // 1. إذا حدد العميل SalesUnitId، نتأكد من صحتها
        // 2. إذا لم يحدد العميل وكان للمنتج وحدات بيع، نختار الوحدة الافتراضية
        // 3. إذا لم تكن هناك أي وحدات، نعتمد الحبة وسعر المنتج الأساسي
        Guid? resolvedSalesUnitId = null;
        string resolvedUnitName = !string.IsNullOrWhiteSpace(product.Unit) ? product.Unit : "حبة";
        decimal unitPrice = product.FinalPrice;

        if (input.SalesUnitId.HasValue)
        {
            var matchedUnit = product.SalesUnits.FirstOrDefault(u => u.SalesUnitId == input.SalesUnitId.Value && u.IsActive);
            if (matchedUnit == null)
            {
                throw new UserFriendlyException("وحدة البيع المحددة غير متوفرة لهذا المنتج.");
            }

            resolvedSalesUnitId = matchedUnit.SalesUnitId;
            resolvedUnitName = matchedUnit.UnitName;
            if (matchedUnit.Price.HasValue && matchedUnit.Price.Value > 0)
            {
                unitPrice = matchedUnit.Price.Value;
            }
        }
        else if (product.SalesUnits != null && product.SalesUnits.Count > 0)
        {
            var defaultUnit = product.SalesUnits.FirstOrDefault(u => u.IsDefault && u.IsActive)
                              ?? product.SalesUnits.FirstOrDefault(u => u.IsActive);

            if (defaultUnit != null)
            {
                resolvedSalesUnitId = defaultUnit.SalesUnitId;
                resolvedUnitName = defaultUnit.UnitName;
                if (defaultUnit.Price.HasValue && defaultUnit.Price.Value > 0)
                {
                    unitPrice = defaultUnit.Price.Value;
                }
            }
        }

        var cart = await FindActiveCartAsync(customer.Id);
        if (cart is null)
        {
            cart = new Cart(GuidGenerator.Create(), customer.Id, product.StoreId);
            await _cartRepository.InsertAsync(cart, autoSave: true);
        }
        else
        {
            EnsureSingleStore(cart, product.StoreId);

            if (cart.Items.Count == 0 && cart.StoreId != product.StoreId)
            {
                cart.ChangeStore(product.StoreId);
            }
        }

        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == product.Id && x.SalesUnitId == resolvedSalesUnitId);
        var requestedQuantity = (existingItem?.Quantity ?? 0) + input.Quantity;
        ValidateQuantity(product, requestedQuantity);

        if (existingItem is null)
        {
            var item = cart.AddItem(
                GuidGenerator.Create(),
                product.Id,
                input.Quantity,
                unitPrice,
                input.Notes,
                resolvedSalesUnitId,
                resolvedUnitName
            );
            await _cartItemRepository.InsertAsync(item);
        }
        else
        {
            existingItem.ChangeQuantity(requestedQuantity, unitPrice);
            await _cartItemRepository.UpdateAsync(existingItem);
        }

        await UpdateStoredTotalsAsync(cart);
        await _cartRepository.UpdateAsync(cart, autoSave: true);

        return await BuildCartDtoAsync(cart, customer.Id);
    }

    public async Task<ShoppingCartDto> UpdateItemQuantityAsync(Guid itemId, UpdateShoppingCartItemQuantityInput input)
    {
        if (input.Quantity <= 0)
        {
            throw new UserFriendlyException("الحد الأدنى للكمية هو 1. لإلغاء شراء المنتج، الرجاء استخدام خيار حذف العنصر من السلة.");
        }

        var customer = await GetCurrentCustomerAsync();
        var cart = await GetActiveCartAsync(customer.Id);
        var item = FindOwnedItem(cart, itemId);

        var productQuery = await _productRepository.WithDetailsAsync(x => x.SalesUnits);
        var product = await AsyncExecuter.FirstOrDefaultAsync(productQuery.Where(x => x.Id == item.ProductId));
        if (product == null)
        {
            throw new EntityNotFoundException(typeof(Product), item.ProductId);
        }

        EnsureProductCanBeOrdered(product);
        ValidateQuantity(product, input.Quantity);

        decimal unitPrice = product.FinalPrice;
        if (item.SalesUnitId.HasValue)
        {
            var matchedUnit = product.SalesUnits.FirstOrDefault(u => u.SalesUnitId == item.SalesUnitId.Value && u.IsActive);
            if (matchedUnit?.Price.HasValue == true && matchedUnit.Price.Value > 0)
            {
                unitPrice = matchedUnit.Price.Value;
            }
        }

        cart.UpdateItemQuantity(item.Id, input.Quantity, unitPrice);
        await _cartItemRepository.UpdateAsync(item);
        await UpdateStoredTotalsAsync(cart);
        await _cartRepository.UpdateAsync(cart, autoSave: true);

        return await BuildCartDtoAsync(cart, customer.Id);
    }

    public async Task<ShoppingCartDto> RemoveItemAsync(Guid itemId)
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await GetActiveCartAsync(customer.Id);
        var item = FindOwnedItem(cart, itemId);

        cart.RemoveItem(item.Id);
        await _cartItemRepository.DeleteAsync(item);
        await UpdateStoredTotalsAsync(cart);
        await _cartRepository.UpdateAsync(cart, autoSave: true);

        return await BuildCartDtoAsync(cart, customer.Id);
    }

    public async Task ClearCartAsync()
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await FindActiveCartAsync(customer.Id);
        if (cart is null)
        {
            return;
        }

        var items = cart.Items.ToList();
        cart.Clear();

        if (items.Count > 0)
        {
            await _cartItemRepository.DeleteManyAsync(items);
        }

        await _cartRepository.UpdateAsync(cart, autoSave: true);
    }

    #endregion

    #region 4. Private Helper Methods

    private async Task<Customer> GetCurrentCustomerAsync()
    {
        var userId = CurrentUser.GetId();
        var customer = await _customerRepository.FindAsync(x => x.UserId == userId);

        if (customer is null)
        {
            throw new UserFriendlyException("عفواً، لم يتم العثور على ملف العميل الخاص بك. الرجاء استكمال بيانات ملفك الشخصي أولاً.");
        }

        return customer;
    }

    private async Task<Cart?> FindActiveCartAsync(Guid customerId)
    {
        var queryable = await _cartRepository.WithDetailsAsync(x => x.Items);
        var query = queryable
            .Where(x => x.CustomerId == customerId && x.IsActive)
            .OrderByDescending(x => x.LastActivityAt);

        return await AsyncExecuter.FirstOrDefaultAsync(query);
    }

    private async Task<Cart> GetActiveCartAsync(Guid customerId)
    {
        var cart = await FindActiveCartAsync(customerId);
        if (cart is null)
        {
            throw new UserFriendlyException("لم يتم العثور على سلة المشتريات.");
        }

        return cart;
    }

    private static CartItem FindOwnedItem(Cart cart, Guid itemId)
    {
        var item = cart.Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null)
        {
            throw new UserFriendlyException("لم يتم العثور على العنصر في سلة المشتريات.");
        }

        return item;
    }

    private static void EnsureSingleStore(Cart cart, Guid productStoreId)
    {
        if (cart.Items.Count > 0 && cart.StoreId != productStoreId)
        {
            throw new UserFriendlyException("عفواً، لا يمكنك إضافة منتجات من متاجر مختلفة في نفس الطلب. الرجاء إتمام الطلب الحالي أو إفراغ السلة أولاً.");
        }
    }

    private static void EnsureProductCanBeOrdered(Product product)
    {
        if (!product.IsActive || !product.IsAvailable)
        {
            throw new UserFriendlyException($"عفواً، المنتج ({product.Name}) غير متاح للطلب حالياً.");
        }
    }

    private static void ValidateQuantity(Product product, int quantity)
    {
        if (quantity < product.MinOrderQuantity)
        {
            throw new UserFriendlyException($"الكمية المطلوبة أقل من الحد الأدنى للطلب وهو {product.MinOrderQuantity}.");
        }

        if (product.MaxOrderQuantity.HasValue && quantity > product.MaxOrderQuantity.Value)
        {
            throw new UserFriendlyException($"الكمية المطلوبة تتجاوز الحد الأقصى للطلب وهو {product.MaxOrderQuantity.Value}.");
        }
    }

    private async Task UpdateStoredTotalsAsync(Cart cart)
    {
        var products = await GetProductsForCartAsync(cart);
        var productById = products.ToDictionary(x => x.Id);

        var snapshots = new List<CartItemPriceSnapshot>();
        foreach (var item in cart.Items)
        {
            if (!productById.TryGetValue(item.ProductId, out var prod))
            {
                continue;
            }

            var (origPrice, currPrice) = ResolvePrices(prod, item.SalesUnitId);
            snapshots.Add(new CartItemPriceSnapshot(item.ProductId, item.SalesUnitId, origPrice, currPrice));
        }

        cart.RecalculateTotals(snapshots);
    }

    private async Task<ShoppingCartDto> BuildCartDtoAsync(Cart cart, Guid customerId, bool updateStoredTotals = false)
    {
        var products = await GetProductsForCartAsync(cart);
        var productById = products.ToDictionary(x => x.Id);

        var dto = _mapper.ToShoppingCartDto(cart);
        dto.CustomerId = customerId;
        dto.StoreId = cart.StoreId;

        var store = await _storeRepository.FindAsync(cart.StoreId);
        dto.StoreName = store?.Name ?? string.Empty;
        dto.Items.Clear();
        dto.Warnings.Clear();
        dto.SubTotal = 0m;
        dto.TotalDiscount = 0m;
        dto.FinalTotal = 0m;

        var snapshots = new List<CartItemPriceSnapshot>();

        foreach (var item in cart.Items.OrderBy(x => x.CreationTime))
        {
            if (!productById.TryGetValue(item.ProductId, out var product))
            {
                var missingProductWarning = $"Product {item.ProductId} is no longer available.";
                dto.Warnings.Add(missingProductWarning);
                dto.HasUnavailableItems = true;
                continue;
            }

            var itemDto = _mapper.ToShoppingCartItemDto(item);
            itemDto.SalesUnitId = item.SalesUnitId;
            itemDto.UnitName = !string.IsNullOrWhiteSpace(item.UnitName) ? item.UnitName : (string.IsNullOrWhiteSpace(product.Unit) ? "حبة" : product.Unit);

            var (originalUnitPrice, currentUnitPrice) = ResolvePrices(product, item.SalesUnitId);
            var unitDiscount = Math.Max(0m, originalUnitPrice - currentUnitPrice);

            itemDto.ProductName = product.Name;
            itemDto.ProductImageUrl = product.MainImageUrl;
            itemDto.UnitPrice = currentUnitPrice;
            itemDto.OriginalUnitPrice = originalUnitPrice;
            itemDto.UnitDiscount = unitDiscount;
            itemDto.TotalPrice = currentUnitPrice * item.Quantity;
            itemDto.IsAvailable = product.IsAvailable;
            itemDto.IsActive = product.IsActive;
            itemDto.MinOrderQuantity = product.MinOrderQuantity;
            itemDto.MaxOrderQuantity = product.MaxOrderQuantity;

            if (!product.IsActive || !product.IsAvailable)
            {
                itemDto.WarningCode = TalabiDomainErrorCodes.CartProductUnavailable;
                itemDto.WarningMessage = "هذا المنتج لم يعد متاحاً للطلب حالياً.";
                dto.Warnings.Add(itemDto.WarningMessage);
                dto.HasUnavailableItems = true;
            }

            dto.SubTotal += originalUnitPrice * item.Quantity;
            dto.TotalDiscount += unitDiscount * item.Quantity;
            dto.FinalTotal += itemDto.TotalPrice;
            dto.Items.Add(itemDto);

            snapshots.Add(new CartItemPriceSnapshot(item.ProductId, item.SalesUnitId, originalUnitPrice, currentUnitPrice));
        }

        if (updateStoredTotals)
        {
            cart.RecalculateTotals(snapshots);
            await _cartRepository.UpdateAsync(cart, autoSave: true);
        }

        return dto;
    }

    private async Task<List<Product>> GetProductsForCartAsync(Cart cart)
    {
        var productIds = cart.Items.Select(x => x.ProductId).Distinct().ToList();
        if (productIds.Count == 0)
        {
            return new List<Product>();
        }

        var queryable = await _productRepository.WithDetailsAsync(x => x.SalesUnits);
        return await AsyncExecuter.ToListAsync(queryable.Where(x => productIds.Contains(x.Id)));
    }

    private static (decimal originalPrice, decimal finalPrice) ResolvePrices(Product product, Guid? salesUnitId)
    {
        if (salesUnitId.HasValue && product.SalesUnits?.Count > 0)
        {
            var unit = product.SalesUnits.FirstOrDefault(u => u.SalesUnitId == salesUnitId.Value);
            if (unit?.Price.HasValue == true && unit.Price.Value > 0)
            {
                return (unit.Price.Value, unit.Price.Value);
            }
        }

        return (product.Price, product.FinalPrice);
    }

    #endregion
}
