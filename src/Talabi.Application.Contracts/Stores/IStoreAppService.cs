using System;
using System.Threading.Tasks;
using Talabi.Stores.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Stores;

/// <summary>
/// واجهة خدمة إدارة المتاجر
/// </summary>
public interface IStoreAppService : IApplicationService
{
    Task<StoreDto> GetAsync(Guid id);

    Task<PagedResultDto<StoreDto>> GetListAsync(GetStoreListInput input);

    Task<StoreDto> CreateAsync(CreateStoreDto input);

    Task<StoreDto> UpdateAsync(Guid id, UpdateStoreDto input);

    Task DeleteAsync(Guid id);
}
