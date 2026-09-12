using System;

namespace Talabi.Notifications.Events;

/// <summary>
/// حدث يتم إطلاقه محلياً (Local Event) عند إنشاء إشعار جديد في النظام.
/// سيلتقطه الـ HttpApi ليرسله عبر SignalR.
/// </summary>
public class NotificationCreatedEvent
{
    public Guid NotificationId { get; set; }
    public Guid RecipientUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public DateTime CreationTime { get; set; }

    public NotificationCreatedEvent(Guid notificationId, Guid recipientUserId, string title, string message, string? actionUrl, DateTime creationTime)
    {
        NotificationId = notificationId;
        RecipientUserId = recipientUserId;
        Title = title;
        Message = message;
        ActionUrl = actionUrl;
        CreationTime = creationTime;
    }
}
