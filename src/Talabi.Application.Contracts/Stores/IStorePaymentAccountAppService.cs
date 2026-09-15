using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Talabi.Stores.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Stores;

/// <summary>
/// واجهة خدمة إدارة حسابات الدفع للمتاجر
/// </summary>
public interface IStorePaymentAccountAppService : IApplicationService
{
    /// <summary>
    /// استرجاع جميع حسابات الدفع لمتجر محدد
    /// </summary>
    Task<List<StorePaymentAccountDto>> GetListByStoreAsync(Guid storeId);

    /// <summary>
    /// استرجاع حساب دفع محدد بواسطة معرفه
    /// </summary>
    Task<StorePaymentAccountDto> GetAsync(Guid id);

    /// <summary>
    /// إضافة حساب دفع جديد لمتجر
    /// </summary>
    Task<StorePaymentAccountDto> CreateAsync(Guid storeId, CreateStorePaymentAccountDto input);

    /// <summary>
    /// تحديث حساب دفع موجود
    /// </summary>
    Task<StorePaymentAccountDto> UpdateAsync(Guid id, CreateStorePaymentAccountDto input);

    /// <summary>
    /// حذف حساب دفع محدد
    /// </summary>
    Task DeleteAsync(Guid id);
}