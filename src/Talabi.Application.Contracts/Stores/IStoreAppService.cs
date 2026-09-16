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

    Task<System.Collections.Generic.List<StorePaymentAccountDto>> GetPaymentAccountsAsync(Guid storeId);

    Task<StorePaymentAccountDto> AddPaymentAccountAsync(CreateStorePaymentAccountDto input);

    Task DeletePaymentAccountAsync(Guid storeId, Guid accountId);

    /// <summary>
    /// زر الفتح والإغلاق السريع للمتجر من قِبل التاجر
    /// </summary>
    Task<StoreDto> ToggleStoreOpenCloseAsync(Guid id);

    /// <summary>
    /// فحص حالة المتجر هل هو مفتوح ومتاح لاستقبال الطلبات الآن
    /// </summary>
    Task<StoreOpenStatusDto> CheckIsOpenAsync(Guid id);

    /// <summary>
    /// استرجاع جدول ساعات وأيام العمل الأسبوعية للمتجر
    /// </summary>
    Task<StoreWorkingHoursDto> GetWorkingHoursAsync(Guid storeId);

    /// <summary>
    /// تحديد وتحديث جدول مواعيد وساعات وأيام العمل الأسبوعية للمتجر
    /// </summary>
    Task<StoreWorkingHoursDto> SetWorkingHoursAsync(Guid storeId, SetStoreWorkingHoursInput input);

    /// <summary>
    /// اعتماد المتجر وتنشيطه في المنصة من قِبل إدارة النظام
    /// </summary>
    Task<StoreDto> ApproveStoreAsync(Guid id);

    /// <summary>
    /// تعليق المتجر وإيقاف نشاطه مؤقتاً من قِبل إدارة النظام
    /// </summary>
    Task<StoreDto> SuspendStoreAsync(Guid id, SuspendStoreInput input);
}
