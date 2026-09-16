using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Permissions;
using Talabi.Products.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Talabi.Products;

/// <summary>
/// خدمة تطبيق إدارة وحدات البيع المستقلة في النظام
/// </summary>
[Authorize]
public class SalesUnitAppService : ApplicationService, ISalesUnitAppService
{
    private readonly IRepository<SalesUnit, Guid> _salesUnitRepository;
    private readonly SalesUnitMapper _mapper;

    public SalesUnitAppService(IRepository<SalesUnit, Guid> salesUnitRepository)
    {
        _salesUnitRepository = salesUnitRepository;
        _mapper = new SalesUnitMapper();
    }

    public async Task<SalesUnitDto> GetAsync(Guid id)
    {
        var unit = await _salesUnitRepository.GetAsync(id);
        return _mapper.ToSalesUnitDto(unit);
    }

    public async Task<PagedResultDto<SalesUnitDto>> GetListAsync(GetSalesUnitListInput input)
    {
        var queryable = await _salesUnitRepository.GetQueryableAsync();

        var query = queryable
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter!) || (x.Code != null && x.Code.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(query);

        var units = await AsyncExecuter.ToListAsync(
            query.OrderBy(input.Sorting ?? nameof(SalesUnit.DisplayOrder))
                 .PageBy(input.SkipCount, input.MaxResultCount)
        );

        return new PagedResultDto<SalesUnitDto>(
            totalCount,
            _mapper.ToSalesUnitDtoList(units)
        );
    }

    [Authorize(TalabiPermissions.SalesUnits.Create)]
    public async Task<SalesUnitDto> CreateAsync(CreateSalesUnitDto input)
    {
        // منع تكرار اسم الوحدة للمستأجر
        var exists = await _salesUnitRepository.AnyAsync(x => x.Name == input.Name.Trim());
        if (exists)
        {
            throw new UserFriendlyException($"وحدة البيع ({input.Name}) موجودة مسبقاً في النظام.");
        }

        var unit = new SalesUnit(
            GuidGenerator.Create(),
            input.Name.Trim(),
            input.Code?.Trim(),
            input.Description?.Trim(),
            input.DisplayOrder,
            input.IsActive,
            CurrentTenant.Id
        );

        await _salesUnitRepository.InsertAsync(unit, autoSave: true);
        Logger.LogInformation("تمت إضافة وحدة بيع جديدة: {Name} (ID: {Id})", unit.Name, unit.Id);

        return _mapper.ToSalesUnitDto(unit);
    }

    [Authorize(TalabiPermissions.SalesUnits.Edit)]
    public async Task<SalesUnitDto> UpdateAsync(Guid id, UpdateSalesUnitDto input)
    {
        var unit = await _salesUnitRepository.GetAsync(id);

        var exists = await _salesUnitRepository.AnyAsync(x => x.Id != id && x.Name == input.Name.Trim());
        if (exists)
        {
            throw new UserFriendlyException($"وحدة البيع ({input.Name}) موجودة مسبقاً في النظام.");
        }

        _mapper.ApplyUpdateDto(input, unit);
        unit.Name = input.Name.Trim();
        unit.Code = input.Code?.Trim();
        unit.Description = input.Description?.Trim();

        await _salesUnitRepository.UpdateAsync(unit, autoSave: true);
        Logger.LogInformation("تم تحديث بيانات وحدة البيع: {Name} (ID: {Id})", unit.Name, unit.Id);

        return _mapper.ToSalesUnitDto(unit);
    }

    [Authorize(TalabiPermissions.SalesUnits.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var unit = await _salesUnitRepository.GetAsync(id);
        await _salesUnitRepository.DeleteAsync(unit);
        Logger.LogInformation("تم حذف وحدة البيع: {Name} (ID: {Id})", unit.Name, id);
    }
}
