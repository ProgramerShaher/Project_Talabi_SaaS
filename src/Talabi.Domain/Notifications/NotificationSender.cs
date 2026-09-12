using System;
using System.Threading.Tasks;
using Talabi.Notifications.Events;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;

namespace Talabi.Notifications;

/// <summary>
/// خدمة إرسال الإشعارات المركزية.
/// تقوم بحفظ الإشعار في قاعدة البيانات ونشر حدث ليتم بثه عبر SignalR.
/// </summary>
public class NotificationSender : DomainService, INotificationSender
{
    private readonly IRepository<AppNotification, Guid> _notificationRepository;
    private readonly IRepository<NotificationType, Guid> _notificationTypeRepository;
    private readonly ILocalEventBus _localEventBus;

    public NotificationSender(
        IRepository<AppNotification, Guid> notificationRepository,
        IRepository<NotificationType, Guid> notificationTypeRepository,
        ILocalEventBus localEventBus)
    {
        _notificationRepository = notificationRepository;
        _notificationTypeRepository = notificationTypeRepository;
        _localEventBus = localEventBus;
    }

    public async Task SendToUserAsync(
        Guid recipientUserId,
        string notificationTypeName,
        string title,
        string message,
        string? relatedEntityName = null,
        Guid? relatedEntityId = null,
        string? actionUrl = null)
    {
        // 1. جلب معرف نوع الإشعار (إذا لم يوجد يتم إنشاء نوع افتراضي أو رمي استثناء)
        var type = await _notificationTypeRepository.FirstOrDefaultAsync(x => x.Name == notificationTypeName);
        if (type == null)
        {
            throw new UserFriendlyException($"نوع الإشعار {notificationTypeName} غير موجود في النظام.");
        }

        // 2. إنشاء الإشعار وحفظه في قاعدة البيانات
        var notification = new AppNotification(
            id: GuidGenerator.Create(),
            recipientUserId: recipientUserId,
            notificationTypeId: type.Id,
            title: title,
            message: message,
            relatedEntityName: relatedEntityName,
            relatedEntityId: relatedEntityId,
            actionUrl: actionUrl,
            sentVia: "Push" // الافتراضي هو إشعار داخل التطبيق
        );

        await _notificationRepository.InsertAsync(notification, autoSave: true);

        // 3. نشر الحدث ليتم التقاطه بواسطة الـ SignalR وبثه للمستخدم فوراً
        var eventData = new NotificationCreatedEvent(
            notificationId: notification.Id,
            recipientUserId: notification.RecipientUserId,
            title: notification.Title,
            message: notification.Message,
            actionUrl: notification.ActionUrl,
            creationTime: Clock.Now
        );

        await _localEventBus.PublishAsync(eventData);
    }
}
