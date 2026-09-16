using System;
using System.Threading.Tasks;
using Talabi.Deliveries.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Deliveries;

/// <summary>
/// واجهة خدمة إدارة مناديب التوصيل
/// </summary>
public interface ICourierAppService : IApplicationService
{
    /// <summary>
    /// جلب بيانات مندوب محدد بواسطة معرفه
    /// </summary>
    Task<CourierDto> GetAsync(Guid id);

    /// <summary>
    /// جلب بيانات المندوب بواسطة معرف المستخدم المرتبط
    /// </summary>
    Task<CourierDto> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// جلب الملف الشخصي للمندوب الحالي المسجل دخوله
    /// </summary>
    Task<CourierDto> GetMyProfileAsync();

    /// <summary>
    /// جلب قائمة المناديب مع الفلترة والتقسيم
    /// </summary>
    Task<PagedResultDto<CourierDto>> GetListAsync(GetCourierListInput input);

    /// <summary>
    /// تسجيل مندوب توصيل جديد
    /// </summary>
    Task<CourierDto> CreateAsync(CreateCourierDto input);

    /// <summary>
    /// تحديث بيانات مندوب التوصيل ومركبته
    /// </summary>
    Task<CourierDto> UpdateAsync(Guid id, UpdateCourierDto input);

    /// <summary>
    /// تحديث حالة جاهزية وتوافر المندوب لاستقبال طلبات جديدة
    /// </summary>
    Task<CourierDto> SetAvailabilityAsync(bool isAvailable);

    /// <summary>
    /// تحديث حالة اتصال المندوب بالتطبيق (Online/Offline)
    /// </summary>
    Task<CourierDto> SetOnlineStatusAsync(bool isOnline);

    /// <summary>
    /// تحديث الموقع الجغرافي اللحظي للمندوب عبر إحداثيات GPS
    /// </summary>
    Task<CourierDto> UpdateLocationAsync(UpdateCourierLocationInput input);
}
