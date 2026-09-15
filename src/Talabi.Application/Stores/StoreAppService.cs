using System;
using System.Collections.Generic;
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
/// خدمة إدارة المتاجر
/// </summary>
[Authorize]
public class StoreAppService : ApplicationService, IStoreAppService
{
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly IRepository<StorePaymentAccount, Guid> _paymentAccountRepository;
    private readonly StoreMapper _mapper;

    public StoreAppService(
        IRepository<Store, Guid> storeRepository,
        IRepository<StorePaymentAccount, Guid> paymentAccountRepository)
    {
        _storeRepository = storeRepository;
        _paymentAccountRepository = paymentAccountRepository;
        _mapper = new StoreMapper();
    }

    public async Task<StoreDto> GetAsync(Guid id)
    {
        var store = await _storeRepository.GetAsync(id);
        return _mapper.ToStoreDto(store);
    }

    public async Task<PagedResultDto<StoreDto>> GetListAsync(GetStoreListInput input)
    {
        var queryable = await _storeRepository.GetQueryableAsync();

        var filteredQuery = queryable
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!) || x.Slug.Contains(input.Filter!))
            .WhereIf(input.StoreTypeId.HasValue, x => x.StoreTypeId == input.StoreTypeId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(input.MinRating.HasValue, x => x.Rating >= input.MinRating);

        var totalCount = await AsyncExecuter.CountAsync(filteredQuery);

        var items = await AsyncExecuter.ToListAsync(
            filteredQuery
                .OrderByDescending(x => x.CreationTime)
                .PageBy(input)
        );

        var dtos = _mapper.ToStoreDtoList(items);
        return new PagedResultDto<StoreDto>(totalCount, dtos);
    }

    [Authorize(TalabiPermissions.Stores.Create)]
    public async Task<StoreDto> CreateAsync(CreateStoreDto input)
    {
        // التحقق من تكرار الرابط اللطيف (Slug) في النظام
        var slugExists = await _storeRepository.AnyAsync(x => x.Slug == input.Slug);
        if (slugExists)
        {
            throw new UserFriendlyException($"الرابط اللطيف ({input.Slug}) مسجل مسبقاً لمتجر آخر، يرجى اختيار رابط مختلف.");
        }

        var store = new Store(
            GuidGenerator.Create(),
            input.OwnerId,
            input.StoreTypeId,
            input.Name,
            input.Slug,
            input.Phone,
            input.Address,
            input.Latitude,
            input.Longitude,
            CurrentTenant.Id
        );
        _mapper.ApplyCreateDto(input, store);
        
        // تعيين القيم الافتراضية
        store.Status = StoreStatus.PendingApproval;
        store.IsActive = true;
        store.TenantId = CurrentTenant.Id;

        await _storeRepository.InsertAsync(store);
        return _mapper.ToStoreDto(store);
    }

    [Authorize(TalabiPermissions.Stores.Edit)]
    public async Task<StoreDto> UpdateAsync(Guid id, UpdateStoreDto input)
    {
        var store = await _storeRepository.GetAsync(id);

        _mapper.ApplyUpdateDto(input, store);

        await _storeRepository.UpdateAsync(store);
        return _mapper.ToStoreDto(store);
    }

    [Authorize(TalabiPermissions.Stores.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _storeRepository.DeleteAsync(id);
    }

    public async Task<List<StorePaymentAccountDto>> GetPaymentAccountsAsync(Guid storeId)
    {
        var accounts = await _paymentAccountRepository.GetListAsync(x => x.StoreId == storeId);
        
        return accounts.Select(a => new StorePaymentAccountDto
        {
            Id = a.Id,
            StoreId = a.StoreId,
            ProviderName = a.ProviderName,
            AccountNumber = a.AccountNumber,
            AccountName = a.AccountName,
            IsActive = a.IsActive,
            Notes = a.Notes
        }).ToList();
    }

    [Authorize(TalabiPermissions.Stores.Edit)]
    public async Task<StorePaymentAccountDto> AddPaymentAccountAsync(CreateStorePaymentAccountDto input)
    {
        var account = new StorePaymentAccount(
            GuidGenerator.Create(),
            input.StoreId,
            input.ProviderName,
            input.AccountNumber,
            input.AccountName,
            input.IsActive,
            input.Notes
        );

        await _paymentAccountRepository.InsertAsync(account);

        return new StorePaymentAccountDto
        {
            Id = account.Id,
            StoreId = account.StoreId,
            ProviderName = account.ProviderName,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            IsActive = account.IsActive,
            Notes = account.Notes
        };
    }

    [Authorize(TalabiPermissions.Stores.Edit)]
    public async Task DeletePaymentAccountAsync(Guid storeId, Guid accountId)
    {
        var account = await _paymentAccountRepository.GetAsync(accountId);
        if (account.StoreId != storeId)
        {
            throw new UserFriendlyException("هذا الحساب لا يتبع لهذا المتجر");
        }

        await _paymentAccountRepository.DeleteAsync(accountId);
    }
}
