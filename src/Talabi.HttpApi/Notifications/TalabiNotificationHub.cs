using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.AspNetCore.SignalR;

namespace Talabi.Notifications;

/// <summary>
/// Hub الخاص بالإشعارات اللحظية.
/// يرث من AbpHub ليدعم مصادقة ABP بشكل تلقائي.
/// </summary>
[Authorize]
public class TalabiNotificationHub : AbpHub
{
    public override async Task OnConnectedAsync()
    {
        // يمكن تسجيل دخول المستخدم أو إضافته لـ Group إذا لزم الأمر
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(System.Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
