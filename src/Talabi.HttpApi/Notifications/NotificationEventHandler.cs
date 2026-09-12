using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Talabi.Notifications.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Talabi.Notifications;

/// <summary>
/// مستمع للأحداث يلتقط حدث "إنشاء إشعار جديد" ويقوم ببثه عبر SignalR للمستخدم المعني.
/// يتم وضع هذه الكلاس في طبقة الـ HttpApi لأنها تتطلب وصولاً للـ HubContext الخاص بـ ASP.NET Core SignalR.
/// </summary>
public class NotificationEventHandler : ILocalEventHandler<NotificationCreatedEvent>, ITransientDependency
{
    private readonly IHubContext<TalabiNotificationHub> _hubContext;

    public NotificationEventHandler(IHubContext<TalabiNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task HandleEventAsync(NotificationCreatedEvent eventData)
    {
        // بث الإشعار للمستخدم المستهدف فقط باستخدام UserIdentifier (الذي يعتمد على ClaimTypes.NameIdentifier)
        await _hubContext.Clients.User(eventData.RecipientUserId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                eventData.NotificationId,
                eventData.Title,
                eventData.Message,
                eventData.ActionUrl,
                eventData.CreationTime
            });
    }
}
