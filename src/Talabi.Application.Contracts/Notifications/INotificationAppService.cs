using System;
using System.Threading.Tasks;
using Talabi.Notifications.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Notifications;

/// <summary>
/// واجهة خدمة جلب وإدارة الإشعارات من قبل المستخدم (Front-End)
/// </summary>
public interface INotificationAppService : IApplicationService
{
    /// <summary>
    /// جلب قائمة إشعارات المستخدم الحالي مع إمكانية الفلترة (مثال: الغير مقروءة فقط)
    /// </summary>
    Task<PagedResultDto<AppNotificationDto>> GetListAsync(GetNotificationListInput input);

    /// <summary>
    /// جلب عدد الإشعارات الغير مقروءة لعرضها فوق أيقونة الجرس 🔔
    /// </summary>
    Task<int> GetUnreadCountAsync();

    /// <summary>
    /// تحديث حالة إشعار محدد إلى "مقروء"
    /// </summary>
    Task MarkAsReadAsync(Guid id);

    /// <summary>
    /// تحديث جميع إشعارات المستخدم الحالي إلى "مقروءة"
    /// </summary>
    Task MarkAllAsReadAsync();
}
