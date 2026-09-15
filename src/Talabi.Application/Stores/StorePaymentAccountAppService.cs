using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Talabi.Stores.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Talabi.Stores;

/// <summary>
/// خدمة التطبيق لإدارة حسابات الدفع الخاصة بالمتاجر
/// </summary>
[Authorize]
public class StorePaymentAccountAppService : ApplicationService, IStorePaymentAccountAppService
{
    private readonly IRepository<StorePaymentAccount, Guid> _paymentAccountRepository;
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly StorePaymentAccountMapper _mapper;

    /// <summary>
    /// منشئ الفئة لتهيئة الخدمة مع المستودعات المطلوبة
    /// </summary>
    public StorePaymentAccountAppService(
        IRepository<StorePaymentAccount, Guid> paymentAccountRepository,
        IRepository<Store, Guid> storeRepository)
    {
        _paymentAccountRepository = paymentAccountRepository;
        _storeRepository = storeRepository;
        _mapper = new StorePaymentAccountMapper();
    }

    /// <summary>
    /// التحقق من صلاحية المستخدم الحالي لإدارة المتجر
    /// </summary>
    private async Task CheckStoreOwnershipAsync(Guid storeId)
    {
        var store = await _storeRepository.GetAsync(storeId);
        // تم تعطيل التقييد بالصلاحيات مؤقتاً بناءً على طلبك لتسهيل التطوير والاختبار
        /*
        if (store.OwnerId != CurrentUser.GetId())
        {
            // في نظام حقيقي قد يتم التحقق أيضاً من جدول StoreUsers
            throw new UserFriendlyException("غير مصرح لك بإدارة حسابات الدفع لهذا المتجر.");
        }
        */
    }

    /// <summary>
    /// استرجاع جميع حسابات الدفع لمتجر محدد
    /// </summary>
    public async Task<List<StorePaymentAccountDto>> GetListByStoreAsync(Guid storeId)
    {
        await CheckStoreOwnershipAsync(storeId);
        
        var accounts = await _paymentAccountRepository.GetListAsync(x => x.StoreId == storeId);
        return _mapper.ToStorePaymentAccountDtoList(accounts);
    }

    /// <summary>
    /// استرجاع حساب دفع محدد بواسطة معرفه
    /// </summary>
    public async Task<StorePaymentAccountDto> GetAsync(Guid id)
    {
        var account = await _paymentAccountRepository.GetAsync(id);
        await CheckStoreOwnershipAsync(account.StoreId);
        
        return _mapper.ToStorePaymentAccountDto(account);
    }

    /// <summary>
    /// إضافة حساب دفع جديد لمتجر
    /// </summary>
    public async Task<StorePaymentAccountDto> CreateAsync(Guid storeId, CreateStorePaymentAccountDto input)
    {
        await CheckStoreOwnershipAsync(storeId);

        var account = new StorePaymentAccount(
            GuidGenerator.Create(),
            storeId,
            input.ProviderName,
            input.AccountNumber,
            input.AccountName,
            input.IsActive,
            input.Notes
        );

        await _paymentAccountRepository.InsertAsync(account);
        return _mapper.ToStorePaymentAccountDto(account);
    }

    /// <summary>
    /// تحديث حساب دفع موجود
    /// </summary>
    public async Task<StorePaymentAccountDto> UpdateAsync(Guid id, CreateStorePaymentAccountDto input)
    {
        var account = await _paymentAccountRepository.GetAsync(id);
        await CheckStoreOwnershipAsync(account.StoreId);

        _mapper.ApplyCreateUpdateDto(input, account);
        
        await _paymentAccountRepository.UpdateAsync(account);
        return _mapper.ToStorePaymentAccountDto(account);
    }

    /// <summary>
    /// حذف حساب دفع محدد
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var account = await _paymentAccountRepository.GetAsync(id);
        await CheckStoreOwnershipAsync(account.StoreId);

        await _paymentAccountRepository.DeleteAsync(account);
    }
}
