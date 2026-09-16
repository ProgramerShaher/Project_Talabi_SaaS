using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Talabi.Notifications.Events;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Identity;

namespace Talabi.Notifications;

/// <summary>
/// خدمة إرسال الإشعارات المركزية.
/// تقوم بحفظ الإشعارات في قاعدة البيانات ونشر أحداث ليتم بثها عبر SignalR في الوقت اللحظي.
/// </summary>
public class NotificationSender : DomainService, INotificationSender
{
    private readonly IRepository<AppNotification, Guid> _notificationRepository;
    private readonly IRepository<NotificationType, Guid> _notificationTypeRepository;
    private readonly ILocalEventBus _localEventBus;
    private readonly IIdentityUserRepository _identityUserRepository;

    public NotificationSender(
        IRepository<AppNotification, Guid> notificationRepository,
        IRepository<NotificationType, Guid> notificationTypeRepository,
        ILocalEventBus localEventBus,
        IIdentityUserRepository identityUserRepository)
    {
        _notificationRepository = notificationRepository;
        _notificationTypeRepository = notificationTypeRepository;
        _localEventBus = localEventBus;
        _identityUserRepository = identityUserRepository;
    }

    /// <summary>
    /// إرسال إشعار لمستخدم محدد وبثه لحظياً (Real-Time).
    /// </summary>
    public async Task SendToUserAsync(
        Guid recipientUserId,
        string notificationTypeName,
        string title,
        string message,
        string? relatedEntityName = null,
        Guid? relatedEntityId = null,
        string? actionUrl = null)
    {
        await SendToUsersAsync(
            new[] { recipientUserId },
            notificationTypeName,
            title,
            message,
            relatedEntityName,
            relatedEntityId,
            actionUrl
        );
    }

    /// <summary>
    /// إرسال إشعار لقائمة من المستخدمين وبثه لحظياً
    /// </summary>
    public async Task SendToUsersAsync(
        IEnumerable<Guid> recipientUserIds,
        string notificationTypeName,
        string title,
        string message,
        string? relatedEntityName = null,
        Guid? relatedEntityId = null,
        string? actionUrl = null)
    {
        if (recipientUserIds == null) return;
        var distinctIds = recipientUserIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (!distinctIds.Any()) return;

        var type = await GetOrCreateNotificationTypeAsync(notificationTypeName, title);

        var notifications = new List<AppNotification>();
        foreach (var userId in distinctIds)
        {
            notifications.Add(new AppNotification(
                id: GuidGenerator.Create(),
                recipientUserId: userId,
                notificationTypeId: type.Id,
                title: title,
                message: message,
                relatedEntityName: relatedEntityName,
                relatedEntityId: relatedEntityId,
                actionUrl: actionUrl,
                sentVia: "Push"
            ));
        }

        await _notificationRepository.InsertManyAsync(notifications, autoSave: true);

        foreach (var notif in notifications)
        {
            var eventData = new NotificationCreatedEvent(
                notificationId: notif.Id,
                recipientUserId: notif.RecipientUserId,
                title: notif.Title,
                message: notif.Message,
                actionUrl: notif.ActionUrl,
                creationTime: Clock.Now
            );

            await _localEventBus.PublishAsync(eventData);
        }
    }

    /// <summary>
    /// إرسال إشعار لجميع مدراء إدارة المنصة (Admin Role) وبثه لحظياً
    /// </summary>
    public async Task SendToAdminsAsync(
        string notificationTypeName,
        string title,
        string message,
        string? relatedEntityName = null,
        Guid? relatedEntityId = null,
        string? actionUrl = null)
    {
        try
        {
            var adminUsers = await _identityUserRepository.GetListByNormalizedRoleNameAsync("ADMIN");
            if (adminUsers != null && adminUsers.Any())
            {
                await SendToUsersAsync(
                    adminUsers.Select(u => u.Id),
                    notificationTypeName,
                    title,
                    message,
                    relatedEntityName,
                    relatedEntityId,
                    actionUrl
                );
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to send notification {NotificationType} to platform administrators.", notificationTypeName);
        }
    }

    private async Task<NotificationType> GetOrCreateNotificationTypeAsync(string notificationTypeName, string title)
    {
        var type = await _notificationTypeRepository.FirstOrDefaultAsync(x => x.Name == notificationTypeName);
        if (type == null)
        {
            type = new NotificationType(
                id: GuidGenerator.Create(),
                name: notificationTypeName,
                displayName: title,
                isActive: true,
                icon: "fas fa-bell",
                color: "#3498db"
            );
            await _notificationTypeRepository.InsertAsync(type, autoSave: true);
        }
        return type;
    }
}
