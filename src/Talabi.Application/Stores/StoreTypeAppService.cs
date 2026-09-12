using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Talabi.Permissions;
using Talabi.Stores.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Talabi.Stores;

/// <summary>
/// خدمة إدارة أنواع المتاجر
/// </summary>
[Authorize(TalabiPermissions.StoreTypes.Default)]
public class StoreTypeAppService : ApplicationService, IStoreTypeAppService
{
    private readonly IRepository<StoreType, Guid> _storeTypeRepository;
    private readonly StoreTypeMapper _mapper;

    public StoreTypeAppService(IRepository<StoreType, Guid> storeTypeRepository)
    {
        _storeTypeRepository = storeTypeRepository;
        _mapper = new StoreTypeMapper();
    }

    public async Task<StoreTypeDto> GetAsync(Guid id)
    {
        var storeType = await _storeTypeRepository.GetAsync(id);
        return _mapper.ToStoreTypeDto(storeType);
    }

    public async Task<PagedResultDto<StoreTypeDto>> GetListAsync(GetStoreTypeListInput input)
    {
        var queryable = await _storeTypeRepository.GetQueryableAsync();

        var filteredQuery = queryable
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        var totalCount = await AsyncExecuter.CountAsync(filteredQuery);

        var items = await AsyncExecuter.ToListAsync(
            filteredQuery
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.CreationTime)
                .PageBy(input)
        );

        var dtos = _mapper.ToStoreTypeDtoList(items);
        return new PagedResultDto<StoreTypeDto>(totalCount, dtos);
    }

    [Authorize(TalabiPermissions.StoreTypes.Create)]
    public async Task<StoreTypeDto> CreateAsync(CreateStoreTypeDto input)
    {
        // التحقق من تكرار اسم النوع
        var exists = await _storeTypeRepository.AnyAsync(x => x.Name == input.Name);
        if (exists)
        {
            throw new UserFriendlyException($"نوع المتجر ({input.Name}) مسجل مسبقاً.");
        }

        var storeType = new StoreType(
            GuidGenerator.Create(),
            input.Name,
            input.DisplayOrder,
            input.IsActive
        );
        _mapper.ApplyCreateDto(input, storeType);

        await _storeTypeRepository.InsertAsync(storeType);
        return _mapper.ToStoreTypeDto(storeType);
    }

    [Authorize(TalabiPermissions.StoreTypes.Edit)]
    public async Task<StoreTypeDto> UpdateAsync(Guid id, UpdateStoreTypeDto input)
    {
        var storeType = await _storeTypeRepository.GetAsync(id);

        // التحقق من تكرار الاسم مع استثناء النوع الحالي
        var exists = await _storeTypeRepository.AnyAsync(x => x.Id != id && x.Name == input.Name);
        if (exists)
        {
            throw new UserFriendlyException($"نوع المتجر ({input.Name}) مسجل مسبقاً.");
        }

        _mapper.ApplyUpdateDto(input, storeType);

        await _storeTypeRepository.UpdateAsync(storeType);
        return _mapper.ToStoreTypeDto(storeType);
    }

    [Authorize(TalabiPermissions.StoreTypes.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        // التحقق من وجود متاجر مرتبطة بهذا النوع
        // بما أن العلاقة Restrict، لا يمكن الحذف إذا كان هناك متاجر، لكن سنترك EF Core يرمي الخطأ أو نعالجه هنا
        await _storeTypeRepository.DeleteAsync(id);
    }
}
