using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Talabi.Notifications.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Talabi.Notifications;

[Authorize]
public class NotificationAppService : ApplicationService, INotificationAppService
{
    private readonly IRepository<AppNotification, Guid> _notificationRepository;
    private readonly IRepository<NotificationType, Guid> _notificationTypeRepository;

    public NotificationAppService(
        IRepository<AppNotification, Guid> notificationRepository,
        IRepository<NotificationType, Guid> notificationTypeRepository)
    {
        _notificationRepository = notificationRepository;
        _notificationTypeRepository = notificationTypeRepository;
    }

    public async Task<PagedResultDto<AppNotificationDto>> GetListAsync(GetNotificationListInput input)
    {
        var userId = CurrentUser.GetId();

        // جلب الإشعارات الخاصة بالمستخدم الحالي فقط
        var query = await _notificationRepository.GetQueryableAsync();
        
        query = query.Where(x => x.RecipientUserId == userId);

        if (input.IsRead.HasValue)
        {
            query = query.Where(x => x.IsRead == input.IsRead.Value);
        }

        if (input.NotificationTypeId.HasValue)
        {
            query = query.Where(x => x.NotificationTypeId == input.NotificationTypeId.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        // ترتيب الأحدث أولاً
        var notifications = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        // جلب أسماء أنواع الإشعارات
        var typeIds = notifications.Select(x => x.NotificationTypeId).Distinct().ToList();
        var types = await _notificationTypeRepository.GetListAsync(x => typeIds.Contains(x.Id));
        var typeDict = types.ToDictionary(x => x.Id, x => x.DisplayName);

        var dtos = notifications.Select(n => new AppNotificationDto
        {
            Id = n.Id,
            RecipientUserId = n.RecipientUserId,
            NotificationTypeId = n.NotificationTypeId,
            NotificationTypeName = typeDict.ContainsKey(n.NotificationTypeId) ? typeDict[n.NotificationTypeId] : string.Empty,
            Title = n.Title,
            Message = n.Message,
            RelatedEntityName = n.RelatedEntityName,
            RelatedEntityId = n.RelatedEntityId,
            ActionUrl = n.ActionUrl,
            IsRead = n.IsRead,
            ReadAt = n.ReadAt,
            SentVia = n.SentVia,
            CreationTime = n.CreationTime
        }).ToList();

        return new PagedResultDto<AppNotificationDto>(totalCount, dtos);
    }

    public async Task<int> GetUnreadCountAsync()
    {
        var userId = CurrentUser.GetId();
        var query = await _notificationRepository.GetQueryableAsync();
        return await AsyncExecuter.CountAsync(query.Where(x => x.RecipientUserId == userId && !x.IsRead));
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        var userId = CurrentUser.GetId();
        var notification = await _notificationRepository.GetAsync(id);

        // تأكد أن الإشعار يخص المستخدم
        if (notification.RecipientUserId != userId)
        {
            throw new UserFriendlyException("غير مصرح لك بتعديل هذا الإشعار");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task MarkAllAsReadAsync()
    {
        var userId = CurrentUser.GetId();
        var unreadNotifications = await _notificationRepository.GetListAsync(x => x.RecipientUserId == userId && !x.IsRead);

        var now = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        if (unreadNotifications.Any())
        {
            await _notificationRepository.UpdateManyAsync(unreadNotifications);
        }
    }
}
