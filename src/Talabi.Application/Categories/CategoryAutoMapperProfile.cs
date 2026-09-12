using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Talabi.Categories.Dtos;

namespace Talabi.Categories;

/// <summary>
/// تهيئة ربط كيانات التصنيفات مع كائنات نقل البيانات (Mapperly)
/// </summary>
[Mapper]
public partial class CategoryMapper
{
    /// <summary>
    /// تحويل كيان التصنيف إلى كائن العرض
    /// </summary>
    [MapProperty(nameof(Category.Id), nameof(CategoryDto.Id))]
    public partial CategoryDto ToCategoryDto(Category source);

    /// <summary>
    /// تحويل قائمة كيانات التصنيف إلى قائمة كائنات العرض
    /// </summary>
    public partial List<CategoryDto> ToCategoryDtoList(List<Category> source);

    /// <summary>
    /// تحويل كائن الإنشاء إلى كيان التصنيف (جزئي - يُستكمل يدوياً في الخدمة)
    /// </summary>
    [MapperIgnoreTarget(nameof(Category.Id))]
    [MapperIgnoreTarget(nameof(Category.Children))]
    [MapperIgnoreTarget(nameof(Category.Parent))]
    [MapperIgnoreTarget(nameof(Category.Level))]
    [MapperIgnoreTarget(nameof(Category.Path))]
    public partial void ApplyCreateDto(CreateCategoryDto source, Category target);

    /// <summary>
    /// تحويل كيان تصنيف المتجر إلى كائن العرض
    /// </summary>
    public partial StoreCategoryDto ToStoreCategoryDto(StoreCategory source);

    /// <summary>
    /// تحويل قائمة كيانات تصنيفات المتاجر
    /// </summary>
    public partial List<StoreCategoryDto> ToStoreCategoryDtoList(List<StoreCategory> source);
}
