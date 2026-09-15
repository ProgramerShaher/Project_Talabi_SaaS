using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Talabi.Notifications;

/// <summary>
/// يزرع (Seeds) أنواع الإشعارات الأساسية في النظام عند تهيئة قاعدة البيانات.
/// </summary>
public class NotificationTypeDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<NotificationType, Guid> _notificationTypeRepository;
    private readonly IGuidGenerator _guidGenerator;

    public NotificationTypeDataSeederContributor(
        IRepository<NotificationType, Guid> notificationTypeRepository,
        IGuidGenerator guidGenerator)
    {
        _notificationTypeRepository = notificationTypeRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedNotificationTypeAsync("OrderStatusChanged", "تحديث حالة الطلب", "fas fa-box-open", "#3498db");
        await SeedNotificationTypeAsync("OrderAssignedToDriver", "تكليف بطلب جديد", "fas fa-motorcycle", "#f39c12");
        await SeedNotificationTypeAsync("SystemAnnouncement", "إعلان إداري", "fas fa-bullhorn", "#e74c3c");
        await SeedNotificationTypeAsync("OrderDelivered", "تم التوصيل", "fas fa-check-circle", "#2ecc71");
        await SeedNotificationTypeAsync("OrderAccepted", "تم قبول الطلب", "fas fa-check-circle", "#27ae60");
        await SeedNotificationTypeAsync("OrderRejected", "تم رفض الطلب", "fas fa-times-circle", "#c0392b");
        await SeedNotificationTypeAsync("OrderCancelled", "تم إلغاء الطلب", "fas fa-ban", "#7f8c8d");
    }

    private async Task SeedNotificationTypeAsync(string name, string displayName, string icon, string color)
    {
        if (await _notificationTypeRepository.FirstOrDefaultAsync(x => x.Name == name) == null)
        {
            await _notificationTypeRepository.InsertAsync(
                new NotificationType(
                    id: _guidGenerator.Create(),
                    name: name,
                    displayName: displayName,
                    isActive: true,
                    icon: icon,
                    color: color
                ),
                autoSave: true
            );
        }
    }
}
