using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Talabi.Stores.Dtos;

namespace Talabi.Stores;

/// <summary>
/// إعدادات Mapperly الخاصة بأنواع المتاجر
/// </summary>
[Mapper]
public partial class StoreTypeMapper
{
    public partial StoreTypeDto ToStoreTypeDto(StoreType source);

    public partial List<StoreTypeDto> ToStoreTypeDtoList(List<StoreType> source);

    [MapperIgnoreTarget(nameof(StoreType.Id))]
    [MapperIgnoreTarget(nameof(StoreType.Stores))]
    public partial void ApplyCreateDto(CreateStoreTypeDto source, StoreType target);

    [MapperIgnoreTarget(nameof(StoreType.Id))]
    [MapperIgnoreTarget(nameof(StoreType.Stores))]
    public partial void ApplyUpdateDto(UpdateStoreTypeDto source, StoreType target);
}
