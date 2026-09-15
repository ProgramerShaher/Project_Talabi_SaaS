# Shopping Cart Module Implementation Plan

## 1. Current Project Findings

- The real Talabi solution is located at `C:\Users\shaher\source\repos\ProgramerShaher\Project_Talabi_SaaS`.
- The active `cwd` provided to Codex, `C:\Users\shaher\source\repos\Project_Talabi_SaaS`, is empty, so implementation should target the real solution path above.
- The project already contains an early cart model:
  - `Talabi.Domain/Carts/Cart.cs`
  - `Talabi.Domain/Carts/CartItem.cs`
  - `Talabi.Application.Contracts/Carts/Dtos/CartDtos.cs`
  - `Talabi.Application/Carts/Validators/CartValidators.cs`
  - `Talabi.EntityFrameworkCore/EntityConfigurations/Carts/CartConfiguration.cs`
- `Cart` currently has `CustomerId`, `StoreId`, `IsActive`, `ExpiresAt`, `LastActivityAt`, and `Items`.
- `CartItem` currently has `CartId`, `ProductId`, `Quantity`, `UnitPriceAtAddition`, `Notes`, and `AddedAt`.
- There is no `CartAppService` / `IShoppingCartAppService` implementation yet.
- `Product` already contains the business data needed by the cart:
  - `StoreId`
  - `Price`
  - `Discount`
  - `DiscountType`
  - `FinalPrice`
  - `MainImageUrl`
  - `IsAvailable`
  - `IsActive`
  - `MinOrderQuantity`
  - `MaxOrderQuantity`
- `Store` is an aggregate root with `IsActive`, `Status`, and `MinimumOrderAmount`.
- Mapperly is already configured through `AddMapperlyObjectMapper<TalabiApplicationModule>()`, and current feature mappers use `Riok.Mapperly.Abstractions`.

## 2. Architectural Decision

I will not create a second parallel cart aggregate that duplicates the existing `Cart` tables.

Recommended path:

- Promote the existing `Cart`/`CartItem` aggregate to the requested shopping cart module.
- Rename or expose the application contract as `ShoppingCart` terminology:
  - `IShoppingCartAppService`
  - `ShoppingCartAppService`
  - `ShoppingCartDto`
  - `ShoppingCartItemDto`
  - `AddShoppingCartItemInput`
  - `UpdateShoppingCartItemQuantityInput`
- Keep the database tables as `AppCarts` and `AppCartItems` unless you explicitly want a full table/class rename migration to `AppShoppingCarts` and `AppShoppingCartItems`.

Reason:

- The existing model already expresses the same bounded context and has migrations.
- Duplicating `Cart` and `ShoppingCart` would introduce ambiguity before the order module.
- The API can use the precise shopping cart naming without forcing risky database churn.

Open decision for approval:

- Option A, recommended: keep domain class names `Cart`/`CartItem`, implement `ShoppingCartAppService` over them, and evolve the schema with missing fields.
- Option B: rename domain classes and tables to `ShoppingCart`/`ShoppingCartItem` everywhere and generate a migration. Cleaner naming, higher blast radius.

## 3. Domain Layer Plan

Target files:

- `src/Talabi.Domain/Carts/Cart.cs`
- `src/Talabi.Domain/Carts/CartItem.cs`
- optional `src/Talabi.Domain/Carts/CartManager.cs`
- `src/Talabi.Domain.Shared/TalabiDomainErrorCodes.cs`
- `src/Talabi.Domain.Shared/Carts/CartConsts.cs`

Changes:

- Add financial aggregate fields to `Cart`:
  - `SubTotal`
  - `TotalDiscount`
  - `FinalTotal`
- Use `decimal` for all money values.
- Keep `CustomerId` as the persisted owner identity because existing Talabi customer-facing entities use `CustomerId`.
- Enforce privacy in the application service by resolving `CurrentUser.GetId()` to the current `Customer.Id`.
- Add domain methods to protect invariants:
  - `AddItem(productId, quantity, unitPrice, ...)`
  - `UpdateItemQuantity(itemId, quantity, product limits)`
  - `RemoveItem(itemId)`
  - `Clear()`
  - `RecalculateTotals(product price snapshot)`
- Add `TotalPrice` or calculated behavior to `CartItem`.
- Align `CartItem` terminology with the requirement:
  - current `UnitPriceAtAddition` can be kept as snapshot price, or renamed to `UnitPrice`.
  - `TotalPrice` should be persisted only if the product requires an order-time snapshot; otherwise it can be calculated when mapping.

Important invariant:

- The cart aggregate itself must never accept an item from a store different from `Cart.StoreId`.
- The product belongs to another aggregate, so the application service or a domain manager will load the product, then pass the required primitive facts into the cart aggregate.

## 4. Business Rules Plan

### Single Store Rule

Implementation flow for `AddItemAsync`:

1. Resolve current user to `Customer`.
2. Load product by `ProductId`.
3. Ensure product is active and belongs to the input/store context.
4. Find the current active cart for the customer.
5. If an active cart exists and has items from another store, throw `BusinessException`.
6. The exception will use a stable error code such as:
   - `Talabi:Carts:SingleStoreViolation`
7. The message should tell the client to clear the cart before adding from another store.

Recommended behavior:

- There should be only one active cart per customer, not one active cart per customer/store.
- This makes the single-store rule physically enforceable by the database.
- Change the current unique index from `(CustomerId, StoreId, IsActive)` to a filtered unique index on `(CustomerId, IsActive)` where `IsActive = 1`.

### Live Pricing

Implementation flow for `GetMyCartAsync`:

1. Load the current customer cart with items.
2. Load all products for the item product ids in one query.
3. Build DTO items using current `Product.FinalPrice`, `Product.Price`, and `Product.Discount`.
4. Compute response totals from live product prices.
5. Optionally update persisted cart totals to match live prices before returning.

Decision:

- The cart item may retain `UnitPriceAtAddition` as historical display/debug data.
- The DTO must expose the live current unit price separately, for example `CurrentUnitPrice`.
- Checkout/order creation later should take a fresh immutable snapshot from live product pricing at checkout time.

### Quantity Limits

Implementation flow:

- For add/update, validate final requested quantity against:
  - `product.MinOrderQuantity`
  - `product.MaxOrderQuantity` when present.
- If adding an existing item, validate the combined quantity.
- If quantity is zero in update, remove the item.
- Use `BusinessException` for violations with stable error codes:
  - `Talabi:Carts:QuantityBelowMinimum`
  - `Talabi:Carts:QuantityAboveMaximum`

### Product Availability Warning

Implementation flow for `GetMyCartAsync`:

- If `Product.IsAvailable == false` or `Product.IsActive == false`, keep the item visible in the cart DTO.
- Add item-level warning flags:
  - `IsAvailable`
  - `IsActive`
  - `WarningCode`
  - `WarningMessage`
- Add cart-level flag:
  - `HasUnavailableItems`
- Add cart-level list:
  - `Warnings`

For add/update:

- Do not allow adding unavailable/inactive products.
- Do not allow increasing quantity for unavailable/inactive products.

## 5. Application.Contracts Plan

Target files:

- `src/Talabi.Application.Contracts/Carts/IShoppingCartAppService.cs`
- `src/Talabi.Application.Contracts/Carts/Dtos/ShoppingCartDtos.cs`

Contracts:

- `Task<ShoppingCartDto> GetMyCartAsync()`
- `Task<ShoppingCartDto> AddItemAsync(AddShoppingCartItemInput input)`
- `Task<ShoppingCartDto> UpdateItemQuantityAsync(Guid itemId, UpdateShoppingCartItemQuantityInput input)`
- `Task RemoveItemAsync(Guid itemId)` or `Task<ShoppingCartDto>` if the frontend should refresh immediately
- `Task ClearCartAsync()`

DTO design:

- `ShoppingCartDto`
  - `Id`
  - `CustomerId`
  - `StoreId`
  - `StoreName`
  - `SubTotal`
  - `TotalDiscount`
  - `FinalTotal`
  - `HasUnavailableItems`
  - `Warnings`
  - `Items`
- `ShoppingCartItemDto`
  - `Id`
  - `ProductId`
  - `ProductName`
  - `ProductImageUrl`
  - `Quantity`
  - `UnitPrice`
  - `OriginalUnitPrice`
  - `UnitDiscount`
  - `TotalPrice`
  - `IsAvailable`
  - `MinOrderQuantity`
  - `MaxOrderQuantity`
  - `WarningCode`
  - `WarningMessage`
- `AddShoppingCartItemInput`
  - `ProductId`
  - `Quantity`
  - `Notes`
- `UpdateShoppingCartItemQuantityInput`
  - `Quantity`

Note:

- I recommend removing `StoreId` from add input and deriving it from the product. That prevents client-side spoofing and makes the single-store rule stronger.

## 6. Application Layer Plan

Target files:

- `src/Talabi.Application/Carts/ShoppingCartAppService.cs`
- `src/Talabi.Application/Carts/ShoppingCartMapper.cs`
- `src/Talabi.Application/Carts/Validators/ShoppingCartValidators.cs`

Dependencies:

- `IRepository<Cart, Guid>`
- `IRepository<CartItem, Guid>` if direct item operations are needed
- `IRepository<Product, Guid>`
- `IRepository<Store, Guid>`
- `IRepository<Customer, Guid>`
- Mapperly mapper instance

Service method behavior:

- `GetMyCartAsync`
  - Requires authenticated user.
  - Resolve customer by `CurrentUser.GetId()`.
  - Load active cart and items.
  - If none exists, return an empty cart DTO.
  - Load product and store data in batched queries.
  - Recalculate live totals with `decimal`.
  - Return DTO only.
- `AddItemAsync`
  - Resolve current customer.
  - Load product.
  - Validate product active/available.
  - Find active cart.
  - Create cart if none exists.
  - Enforce single-store rule if an active cart already exists.
  - Validate quantity limits.
  - Add or increment item.
  - Persist and return fresh `GetMyCartAsync`.
- `UpdateItemQuantityAsync`
  - Resolve current customer.
  - Load active cart owned by customer.
  - Find item only inside that cart.
  - Quantity zero removes item.
  - Otherwise validate against live product limits.
  - Persist and return fresh cart.
- `RemoveItemAsync`
  - Resolve current customer.
  - Load active cart owned by customer.
  - Remove item only if it belongs to that cart.
- `ClearCartAsync`
  - Resolve current customer.
  - Load active cart owned by customer.
  - Delete items or mark cart inactive depending on order module expectations.

Privacy rule:

- No method will accept `CustomerId`, `UserId`, or `CartId` from the client for ownership.
- All mutations are scoped by `CurrentUser.GetId()` -> `Customer.Id` -> active cart.

## 7. Mapperly Plan

Mapperly will be used only in `Application`.

Because cart DTOs require live product/store data, mapping will be hybrid:

- Mapperly handles simple entity-to-DTO fields.
- The app service enriches DTOs with product name, image, live price, warnings, and computed totals.

No AutoMapper profile will be added.

## 8. EF Core Plan

Target file:

- `src/Talabi.EntityFrameworkCore/EntityConfigurations/Carts/CartConfiguration.cs`

Changes:

- Add decimal precision:
  - `SubTotal` as `decimal(18,2)`
  - `TotalDiscount` as `decimal(18,2)`
  - `FinalTotal` as `decimal(18,2)`
- Add/check item pricing:
  - `UnitPriceAtAddition` or `UnitPrice` as `decimal(18,2)`
  - optional persisted `TotalPrice` as `decimal(18,2)`
- Add indexes:
  - filtered unique index on active cart per customer.
  - unique index on `(CartId, ProductId)` to prevent duplicate product rows inside one cart.
- Keep cascade delete from cart to items.
- Keep restrict delete from item to product.

Migration:

- Generate an EF Core migration after implementation.
- If using Option A, migration only adds fields/indexes.
- If using Option B, migration also renames tables/classes and requires careful snapshot review.

## 9. Error Codes and Localization

Target files:

- `src/Talabi.Domain.Shared/TalabiDomainErrorCodes.cs`
- `src/Talabi.Domain.Shared/Localization/Talabi/ar.json`
- `src/Talabi.Domain.Shared/Localization/Talabi/en.json`

Error codes:

- `Talabi:Carts:CustomerProfileNotFound`
- `Talabi:Carts:ProductNotFound`
- `Talabi:Carts:ProductUnavailable`
- `Talabi:Carts:SingleStoreViolation`
- `Talabi:Carts:QuantityBelowMinimum`
- `Talabi:Carts:QuantityAboveMaximum`
- `Talabi:Carts:CartItemNotFound`

Use `BusinessException(code)` for business-rule failures.

## 10. Tests Plan

Target project:

- `test/Talabi.Application.Tests`

Scenarios:

- `GetMyCartAsync` returns empty DTO for authenticated customer without cart.
- Adding the first item creates an active cart for the current customer.
- Adding a product from a second store throws `BusinessException`.
- Adding the same product increments quantity and validates the combined quantity.
- Updating below `MinOrderQuantity` throws `BusinessException`.
- Updating above `MaxOrderQuantity` throws `BusinessException`.
- Updating quantity to zero removes the item.
- Removing an item from another user's cart is impossible because the query is scoped to current customer.
- `GetMyCartAsync` returns live price after product `FinalPrice` changes.
- `GetMyCartAsync` flags unavailable products instead of silently hiding them.

## 11. Build and Verification Plan

After approval and implementation:

1. Run `dotnet build Talabi.slnx`.
2. Run focused application/domain tests if available.
3. Review generated EF migration for indexes, decimal precision, and unwanted table churn.
4. Confirm no entities are returned from application services.
5. Confirm no AutoMapper usage is introduced.

## 12. Requested Feedback Before Implementation

Please confirm these two decisions before I write code:

1. Naming strategy:
   - Recommended: keep existing domain/database names `Cart` and `CartItem`, and expose the feature as `ShoppingCartAppService`.
   - Alternative: fully rename the domain and database model to `ShoppingCart` and `ShoppingCartItem`.
2. Active cart rule:
   - Recommended: one active cart per customer globally, which strictly enforces one-store checkout.
   - Alternative: one active cart per customer/store, while application code blocks cross-store additions.

