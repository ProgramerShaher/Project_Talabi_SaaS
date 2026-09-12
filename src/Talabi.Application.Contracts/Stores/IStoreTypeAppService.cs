using System;
using System.Threading.Tasks;
using Talabi.Stores.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Stores;

/// <summary>
/// واجهة خدمة إدارة أنواع المتاجر
/// </summary>
public interface IStoreTypeAppService : IApplicationService
{
    Task<StoreTypeDto> GetAsync(Guid id);

    Task<PagedResultDto<StoreTypeDto>> GetListAsync(GetStoreTypeListInput input);

    Task<StoreTypeDto> CreateAsync(CreateStoreTypeDto input);

    Task<StoreTypeDto> UpdateAsync(Guid id, UpdateStoreTypeDto input);

    Task DeleteAsync(Guid id);
}
