using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Talabi.Products.Dtos;

namespace Talabi.Products;

/// <summary>
/// إعدادات Mapperly الخاصة بالمنتجات
/// </summary>
[Mapper]
public partial class ProductMapper
{
    [MapProperty(nameof(Product.StoreCategory.CustomName), nameof(ProductDto.StoreCategoryName))]
    public partial ProductDto ToProductDto(Product source);

    public partial List<ProductDto> ToProductDtoList(List<Product> source);

    [MapperIgnoreTarget(nameof(Product.Id))]
    [MapperIgnoreTarget(nameof(Product.Store))]
    [MapperIgnoreTarget(nameof(Product.StoreCategory))]
    [MapperIgnoreTarget(nameof(Product.Images))]
    public partial void ApplyCreateDto(CreateProductDto source, Product target);

    [MapperIgnoreTarget(nameof(Product.Id))]
    [MapperIgnoreTarget(nameof(Product.StoreId))]
    [MapperIgnoreTarget(nameof(Product.Store))]
    [MapperIgnoreTarget(nameof(Product.StoreCategory))]
    [MapperIgnoreTarget(nameof(Product.Images))]
    public partial void ApplyUpdateDto(UpdateProductDto source, Product target);
}
