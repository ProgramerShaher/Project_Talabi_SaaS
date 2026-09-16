using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Talabi.Products.Dtos;

namespace Talabi.Products;

/// <summary>
/// إعدادات Mapperly الخاصة بوحدات البيع
/// </summary>
[Mapper]
public partial class SalesUnitMapper
{
    public partial SalesUnitDto ToSalesUnitDto(SalesUnit source);

    public partial List<SalesUnitDto> ToSalesUnitDtoList(List<SalesUnit> source);

    [MapperIgnoreTarget(nameof(SalesUnit.Id))]
    [MapperIgnoreTarget(nameof(SalesUnit.TenantId))]
    public partial void ApplyCreateDto(CreateSalesUnitDto source, SalesUnit target);



    [MapperIgnoreTarget(nameof(SalesUnit.Id))]
    [MapperIgnoreTarget(nameof(SalesUnit.TenantId))]
    public partial void ApplyUpdateDto(UpdateSalesUnitDto source, SalesUnit target);

    public partial ProductSalesUnitDto ToProductSalesUnitDto(ProductSalesUnit source);

    public partial List<ProductSalesUnitDto> ToProductSalesUnitDtoList(List<ProductSalesUnit> source);
}
