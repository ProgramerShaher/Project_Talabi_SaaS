using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Talabi.Customers.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Customers;

/// <summary>
/// واجهة خدمة إدارة عناوين العميل
/// </summary>
public interface ICustomerAddressAppService : IApplicationService
{
    /// <summary>
    /// جلب جميع عناوين العميل الحالي
    /// </summary>
    Task<List<CustomerAddressDto>> GetMyAddressesAsync();

    /// <summary>
    /// جلب تفاصيل عنوان محدد
    /// </summary>
    Task<CustomerAddressDto> GetAsync(Guid id);

    /// <summary>
    /// إضافة عنوان جديد للعميل الحالي
    /// </summary>
    Task<CustomerAddressDto> CreateAsync(CreateUpdateCustomerAddressDto input);

    /// <summary>
    /// تحديث بيانات عنوان للعميل الحالي
    /// </summary>
    Task<CustomerAddressDto> UpdateAsync(Guid id, CreateUpdateCustomerAddressDto input);

    /// <summary>
    /// حذف عنوان
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// تعيين كعنوان افتراضي للتوصيل
    /// </summary>
    Task SetAsDefaultAsync(Guid id);
}
