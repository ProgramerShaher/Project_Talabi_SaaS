namespace Talabi.Products;

/// <summary>
/// ثوابت وقيود حقول المنتجات
/// </summary>
public static class ProductConsts
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 2000;
    public const int MaxShortDescriptionLength = 500;
    public const int MaxSkuLength = 50;
    public const int MaxBarcodeLength = 50;
    public const int MaxUnitLength = 50;
    public const int MaxBrandLength = 100;
    public const int MaxWeightUnitLength = 20;
    public const int MaxOriginLength = 100;
    public const int MaxMainImageUrlLength = 500;
    public const int MaxTagsLength = 500;
}

/// <summary>
/// ثوابت وقيود حقول صور المنتجات
/// </summary>
public static class ProductImageConsts
{
    public const int MaxImageUrlLength = 500;
    public const int MaxThumbnailUrlLength = 500;
    public const int MaxAltTextLength = 200;
}

/// <summary>
/// ثوابت وقيود حقول المخزون
/// </summary>
public static class InventoryConsts
{
    public const int MaxWarehouseLocationLength = 100;
    public const int DefaultMinStockLevel = 5;
    public const int DefaultMaxStockLevel = 1000;
    public const int DefaultReorderLevel = 10;
}
