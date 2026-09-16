using System;
using System.Threading.Tasks;
using Talabi.Deliveries.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Deliveries;

/// <summary>
/// واجهة خدمة إدارة وتتبع مشاوير وعمليات التوصيل
/// </summary>
public interface IDeliveryAppService : IApplicationService
{
    /// <summary>
    /// إسناد وتعيين طلب لمندوب توصيل محدد
    /// </summary>
    Task<DeliveryAssignmentDto> AssignCourierAsync(AssignCourierInput input);

    /// <summary>
    /// جلب المشوار والتوصيلة النشطة حالياً للمندوب المسجل دخوله
    /// </summary>
    Task<DeliveryAssignmentDto?> GetMyActiveDeliveryAsync();

    /// <summary>
    /// جلب قائمة مشاوير المندوب الحالي مع الفلترة حسب الحالة والتقسيم
    /// </summary>
    Task<PagedResultDto<DeliveryAssignmentDto>> GetMyDeliveriesAsync(GetMyDeliveriesInput input);

    /// <summary>
    /// جلب تفاصيل التوصيل لطلب معين بواسطة معرف الطلب
    /// </summary>
    Task<DeliveryAssignmentDto?> GetDeliveryByOrderIdAsync(Guid orderId);

    /// <summary>
    /// قبول المندوب للطلب المسند إليه
    /// </summary>
    Task<DeliveryAssignmentDto> AcceptDeliveryAsync(Guid deliveryAssignmentId);

    /// <summary>
    /// تسجيل استلام المندوب للطلب من المتجر والانطلاق به للعميل
    /// </summary>
    Task<DeliveryAssignmentDto> PickupDeliveryAsync(Guid deliveryAssignmentId);

    /// <summary>
    /// توثيق وإثبات تأكيد تسليم الطلب النهائي للعميل (عبر رمز تحقق OTP أو صورة أو توقيع)
    /// </summary>
    Task<DeliveryAssignmentDto> ConfirmDeliveryAsync(ConfirmDeliveryInput input);

    /// <summary>
    /// تسجيل تعثر أو فشل توصيل الطلب مع ذكر السبب
    /// </summary>
    Task<DeliveryAssignmentDto> FailDeliveryAsync(Guid deliveryAssignmentId, FailDeliveryInput input);
}
