using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace Talabi.Notifications;

/// <summary>
/// واجهة خدمة إرسال الإشعارات المركزية في طبقة الـ Domain.
/// </summary>
public interface INotificationSender : IDomainService
{
    /// <summary>
    /// إرسال إشعار لمستخدم محدد وبثه لحظياً (Real-Time).
    /// </summary>
    Task SendToUserAsync(
        Guid recipientUserId,
        string notificationTypeName,
        string title,
        string message,
        string? relatedEntityName = null,
        Guid? relatedEntityId = null,
        string? actionUrl = null);
}
