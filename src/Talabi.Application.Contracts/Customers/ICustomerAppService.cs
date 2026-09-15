using System;
using System.Threading.Tasks;
using Talabi.Customers.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Customers;

/// <summary>
/// واجهة خدمة العميل لإدارة ملفه الشخصي وعناوينه
/// </summary>
public interface ICustomerAppService : IApplicationService
{
    /// <summary>
    /// إنشاء ملف شخصي للعميل الحالي
    /// </summary>
    Task<CustomerDto> CreateProfileAsync(UpdateCustomerDto input);

    /// <summary>
    /// جلب بيانات الملف الشخصي للعميل الحالي
    /// </summary>
    Task<CustomerDto> GetProfileAsync();

    /// <summary>
    /// تحديث بيانات الملف الشخصي للعميل الحالي
    /// </summary>
    Task<CustomerDto> UpdateProfileAsync(UpdateCustomerDto input);

    /// <summary>
    /// تحديث الصورة الشخصية للعميل
    /// </summary>
    Task<CustomerDto> UpdateAvatarAsync(UpdateCustomerAvatarDto input);
}
