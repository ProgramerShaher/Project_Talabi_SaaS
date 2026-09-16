using System;
using System.Threading.Tasks;
using Talabi.Orders.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Orders;

/// <summary>
/// واجهة خدمة إدارة الطلبات
/// </summary>
public interface IOrderAppService : IApplicationService
{
    /// <summary>
    /// جلب قائمة الطلبات مع الفلترة والتقسيم
    /// </summary>
    Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListInput input);

    /// <summary>
    /// جلب تفاصيل طلب محدد
    /// </summary>
    Task<OrderDto> GetAsync(Guid id);

    /// <summary>
    /// إنشاء طلب جديد بشكل يدوي (للتجار/لوحة التحكم)
    /// </summary>
    Task<OrderDto> PlaceOrderAsync(CreateOrderInput input);

    /// <summary>
    /// إنشاء طلب جديد ودفعه مباشرة من سلة المشتريات للعميل الحالي
    /// </summary>
    Task<OrderDto> CheckoutCartAsync(CheckoutCartInput input);

    /// <summary>
    /// إرفاق أو تحديث رابط صورة إيصال الدفع للطلب مع ربطها بكيانات الدفع
    /// </summary>
    Task<OrderDto> AttachPaymentReceiptAsync(Guid id, string receiptUrl, Guid? mediaFileId = null);

    /// <summary>
    /// تغيير حالة الطلب
    /// </summary>
    Task<OrderDto> ChangeOrderStatusAsync(Guid id, Guid newStatusId, string? notes = null);

    /// <summary>
    /// تعيين مندوب توصيل للطلب
    /// </summary>
    Task<OrderDto> AssignDriverAsync(Guid id, Guid courierId);

    /// <summary>
    /// إلغاء الطلب من قبل العميل
    /// </summary>
    Task<OrderDto> CancelOrderAsync(Guid id, CancelOrderInput input);

    /// <summary>
    /// جلب قائمة طلبات العميل الحالي مع الفلترة حسب (الكل، النشطة، السابقة)
    /// </summary>
    Task<PagedResultDto<OrderDto>> GetMyOrdersAsync(GetMyOrdersInput input);

    /// <summary>
    /// جلب إحصائيات ومؤشرات طلبات العميل الحالي
    /// </summary>
    Task<CustomerOrderStatsDto> GetMyOrderStatsAsync();

    /// <summary>
    /// جلب إحصائيات ومؤشرات أداء طلبات المتجر والمبيعات
    /// </summary>
    Task<StoreOrderStatsDto> GetStoreOrderStatsAsync(Guid storeId);

    /// <summary>
    /// اعتماد وتأكيد استلام الدفعة المالية للطلب من قبل المتجر بعد مراجعة الإيصال
    /// </summary>
    Task<OrderDto> ConfirmOrderPaymentAsync(Guid id, ConfirmOrderPaymentInput? input = null);

    /// <summary>
    /// إعادة الطلب السابق ونقل كافة أصنافه مباشرة إلى سلة المشتريات الحالية
    /// </summary>
    Task<Talabi.Carts.Dtos.ShoppingCartDto> ReorderAsync(Guid id);

    /// <summary>
    /// جلب المسار والخط الزمني لحالات الطلب
    /// </summary>
    Task<System.Collections.Generic.List<OrderStatusHistoryDto>> GetOrderTimelineAsync(Guid id);

    /// <summary>
    /// حذف الطلب من السجل (حذف ناعم Soft Delete مع التحقق من عدم كونه نشطاً)
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// قبول الطلب من قبل المتجر مع إمكانية تحديد وقت التوصيل المتوقع
    /// </summary>
    Task<OrderDto> AcceptOrderAsync(Guid id, AcceptOrderInput? input = null);

    /// <summary>
    /// رفض الطلب من قبل المتجر
    /// </summary>
    Task<OrderDto> RejectOrderAsync(Guid id, RejectOrderInput input);

    /// <summary>
    /// إكمال وتسليم الطلب
    /// </summary>
    Task<OrderDto> CompleteOrderAsync(Guid id, string? notes = null);

    /// <summary>
    /// جلب أسباب الإلغاء والرفض
    /// </summary>
    Task<System.Collections.Generic.List<CancellationReasonDto>> GetCancellationReasonsAsync(bool activeOnly = true);

    /// <summary>
    /// جلب حالات الطلب المتاحة
    /// </summary>
    Task<System.Collections.Generic.List<OrderStatusDto>> GetOrderStatusesAsync();
}
