using System;
using Volo.Abp.Application.Dtos;

namespace Talabi.Notifications.Dtos;

/// <summary>
/// كائن نوع الإشعار
/// </summary>
public class NotificationTypeDto : EntityDto<Guid>
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Template { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    #endregion
}

/// <summary>
/// كائن إشعار المستخدم
/// </summary>
public class AppNotificationDto : CreationAuditedEntityDto<Guid>
{
    #region Properties
    public Guid RecipientUserId { get; set; }
    public Guid NotificationTypeId { get; set; }
    public string NotificationTypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityName { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? SentVia { get; set; }
    #endregion
}

/// <summary>
/// كائن فلترة إشعارات المستخدم
/// </summary>
public class GetNotificationListInput : PagedAndSortedResultRequestDto
{
    #region Properties
    public bool? IsRead { get; set; }
    public Guid? NotificationTypeId { get; set; }
    #endregion
}
