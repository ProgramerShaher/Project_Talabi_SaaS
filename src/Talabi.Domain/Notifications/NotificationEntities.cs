using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Notifications;

/// <summary>
/// كيان نوع أو تصنيف الإشعار (تحديث طلب، عرض خاص، إيداع رصيد...)
/// </summary>
public class NotificationType : Entity<Guid>
{
    /// <summary>
    /// الاسم البرمجي الفريد لنوع الإشعار
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// الاسم الظاهر المعروض للمستخدم
    /// </summary>
    public virtual string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// قالب نص الرسالة الافتراضي (يدعم المتغيرات مثل {0} {1})
    /// </summary>
    public virtual string? Template { get; set; }

    /// <summary>
    /// أيقونة الإشعار
    /// </summary>
    public virtual string? Icon { get; set; }

    /// <summary>
    /// لون تمييز الإشعار
    /// </summary>
    public virtual string? Color { get; set; }

    /// <summary>
    /// هل نوع الإشعار نشط في النظام؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    protected NotificationType()
    {
    }

    public NotificationType(
        Guid id,
        string name,
        string displayName,
        bool isActive = true,
        string? template = null,
        string? icon = null,
        string? color = null)
        : base(id)
    {
        Name = name;
        DisplayName = displayName;
        IsActive = isActive;
        Template = template;
        Icon = icon;
        Color = color;
    }
}

/// <summary>
/// كيان الإشعار المرسل للمستخدم
/// </summary>
public class AppNotification : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// معرف المستخدم المستلم للإشعار
    /// </summary>
    public virtual Guid RecipientUserId { get; set; }

    /// <summary>
    /// معرف نوع الإشعار
    /// </summary>
    public virtual Guid NotificationTypeId { get; set; }

    /// <summary>
    /// عنوان الإشعار
    /// </summary>
    public virtual string Title { get; set; } = string.Empty;

    /// <summary>
    /// نص محتوى رسالة الإشعار
    /// </summary>
    public virtual string Message { get; set; } = string.Empty;

    /// <summary>
    /// اسم الكيان المرتبط (Order, Product, Wallet)
    /// </summary>
    public virtual string? RelatedEntityName { get; set; }

    /// <summary>
    /// معرف الكيان المرتبط
    /// </summary>
    public virtual Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// رابط التوجيه عند النقر على الإشعار في التطبيق
    /// </summary>
    public virtual string? ActionUrl { get; set; }

    /// <summary>
    /// هل تم فتح وقراءة الإشعار؟
    /// </summary>
    public virtual bool IsRead { get; set; }

    /// <summary>
    /// تاريخ ووقت القراءة
    /// </summary>
    public virtual DateTime? ReadAt { get; set; }

    /// <summary>
    /// وسيلة الإرسال (Push, Email, SMS, WhatsApp)
    /// </summary>
    public virtual string? SentVia { get; set; }

    /// <summary>
    /// نوع الإشعار المرتبط
    /// </summary>
    public virtual NotificationType? NotificationType { get; set; }

    protected AppNotification()
    {
    }

    public AppNotification(
        Guid id,
        Guid recipientUserId,
        Guid notificationTypeId,
        string title,
        string message,
        string? relatedEntityName = null,
        Guid? relatedEntityId = null,
        string? actionUrl = null,
        string? sentVia = "Push")
        : base(id)
    {
        RecipientUserId = recipientUserId;
        NotificationTypeId = notificationTypeId;
        Title = title;
        Message = message;
        RelatedEntityName = relatedEntityName;
        RelatedEntityId = relatedEntityId;
        ActionUrl = actionUrl;
        SentVia = sentVia;
        IsRead = false;
    }
}
