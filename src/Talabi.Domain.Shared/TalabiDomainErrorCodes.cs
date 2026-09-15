namespace Talabi;

public static class TalabiDomainErrorCodes
{
    public const string CartCustomerProfileNotFound = "Talabi:Carts:CustomerProfileNotFound";
    public const string CartProductUnavailable = "Talabi:Carts:ProductUnavailable";
    public const string CartSingleStoreViolation = "Talabi:Carts:SingleStoreViolation";
    public const string CartQuantityBelowMinimum = "Talabi:Carts:QuantityBelowMinimum";
    public const string CartQuantityAboveMaximum = "Talabi:Carts:QuantityAboveMaximum";
    public const string CartItemNotFound = "Talabi:Carts:CartItemNotFound";
}
