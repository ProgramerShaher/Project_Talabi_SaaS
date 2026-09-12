using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Talabi.Stores.Dtos;

namespace Talabi.Stores;

/// <summary>
/// إعدادات Mapperly الخاصة بالمتاجر
/// </summary>
[Mapper]
public partial class StoreMapper
{
    [MapProperty(nameof(Store.StoreType.Name), nameof(StoreDto.StoreTypeName))]
    public partial StoreDto ToStoreDto(Store source);

    public partial List<StoreDto> ToStoreDtoList(List<Store> source);

    [MapperIgnoreTarget(nameof(Store.Id))]
    [MapperIgnoreTarget(nameof(Store.TenantId))]
    [MapperIgnoreTarget(nameof(Store.Status))]
    [MapperIgnoreTarget(nameof(Store.IsActive))]
    [MapperIgnoreTarget(nameof(Store.IsFeatured))]
    [MapperIgnoreTarget(nameof(Store.Rating))]
    [MapperIgnoreTarget(nameof(Store.TotalReviews))]
    [MapperIgnoreTarget(nameof(Store.TotalOrders))]
    [MapperIgnoreTarget(nameof(Store.StoreType))]
    [MapperIgnoreTarget(nameof(Store.StoreUsers))]
    public partial void ApplyCreateDto(CreateStoreDto source, Store target);

    [MapperIgnoreTarget(nameof(Store.Id))]
    [MapperIgnoreTarget(nameof(Store.TenantId))]
    [MapperIgnoreTarget(nameof(Store.OwnerId))]
    [MapperIgnoreTarget(nameof(Store.Rating))]
    [MapperIgnoreTarget(nameof(Store.TotalReviews))]
    [MapperIgnoreTarget(nameof(Store.TotalOrders))]
    [MapperIgnoreTarget(nameof(Store.StoreType))]
    [MapperIgnoreTarget(nameof(Store.StoreUsers))]
    public partial void ApplyUpdateDto(UpdateStoreDto source, Store target);
}
