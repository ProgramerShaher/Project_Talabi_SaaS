using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Orders.Dtos;

/// <summary>
/// تصنيف نوع فلترة طلبات العميل
/// </summary>
public enum OrderFilterType
{
    /// <summary>
    /// كافة الطلبات
    /// </summary>
    All = 0,

    /// <summary>
    /// الطلبات النشطة والجارية حالياً (قيد الانتظار، مقبولة، قيد التجهيز، في الطريق)
    /// </summary>
    Active = 1,

    /// <summary>
    /// الطلبات السابقة والمنتهية (تم التسليم، ملغية، مرفوضة)
    /// </summary>
    Past = 2
}

/// <summary>
/// كائن طلب قائمة طلبات العميل الحالي مع الفلترة والتقسيم
/// </summary>
public class GetMyOrdersInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// نوع الفلترة (الكل، النشطة، السابقة)
    /// </summary>
    public OrderFilterType FilterType { get; set; } = OrderFilterType.All;

    /// <summary>
    /// فلترة إضافية حسب حالة محددة إن رغب العميل
    /// </summary>
    public Guid? OrderStatusId { get; set; }
}

/// <summary>
/// كائن إحصائيات طلبات العميل
/// </summary>
public class CustomerOrderStatsDto
{
    /// <summary>
    /// إجمالي عدد طلبات العميل
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// عدد الطلبات النشطة والجارية حالياً
    /// </summary>
    public int ActiveOrdersCount { get; set; }

    /// <summary>
    /// عدد الطلبات المسلمة بنجاح
    /// </summary>
    public int DeliveredOrdersCount { get; set; }

    /// <summary>
    /// عدد الطلبات الملغية أو المرفوضة
    /// </summary>
    public int CancelledOrdersCount { get; set; }

    /// <summary>
    /// إجمالي المبالغ المصروفة على الطلبات المكتملة
    /// </summary>
    public decimal TotalSpent { get; set; }
}

/// <summary>
/// كائن إحصائيات ومؤشرات أداء طلبات المتجر
/// </summary>
public class StoreOrderStatsDto
{
    /// <summary>
    /// إجمالي عدد طلبات المتجر
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// عدد الطلبات المعلقة بانتظار الموافقة (قيد الانتظار)
    /// </summary>
    public int PendingOrdersCount { get; set; }

    /// <summary>
    /// عدد الطلبات المقبولة
    /// </summary>
    public int AcceptedOrdersCount { get; set; }

    /// <summary>
    /// عدد الطلبات قيد التجهيز بالمطبخ/المتجر
    /// </summary>
    public int PreparingOrdersCount { get; set; }

    /// <summary>
    /// عدد الطلبات في الطريق مع المندوب
    /// </summary>
    public int InTransitOrdersCount { get; set; }

    /// <summary>
    /// عدد الطلبات المسلمة بنجاح
    /// </summary>
    public int DeliveredOrdersCount { get; set; }

    /// <summary>
    /// عدد الطلبات الملغية من قبل العميل
    /// </summary>
    public int CancelledOrdersCount { get; set; }

    /// <summary>
    /// عدد الطلبات المرفوضة من المتجر
    /// </summary>
    public int RejectedOrdersCount { get; set; }

    /// <summary>
    /// عدد طلبات اليوم
    /// </summary>
    public int TodayOrdersCount { get; set; }

    /// <summary>
    /// إجمالي المبيعات والإيرادات للطلبات المكتملة
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// إجمالي مبيعات وإيرادات اليوم
    /// </summary>
    public decimal TodayRevenue { get; set; }

    /// <summary>
    /// متوسط قيمة الطلب الواحد
    /// </summary>
    public decimal AverageOrderValue { get; set; }
}

/// <summary>
/// كائن عنصر الخط الزمني وتاريخ حالة الطلب
/// </summary>
public class OrderStatusHistoryDto : CreationAuditedEntityDto<Guid>
{
    /// <summary>
    /// معرف الطلب
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// معرف الحالة
    /// </summary>
    public Guid StatusId { get; set; }

    /// <summary>
    /// الاسم البرمجي للحالة
    /// </summary>
    public string StatusName { get; set; } = string.Empty;

    /// <summary>
    /// الاسم العربي المعروض للحالة
    /// </summary>
    public string StatusDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// لون تمييز الحالة
    /// </summary>
    public string? StatusColor { get; set; }

    /// <summary>
    /// أيقونة الحالة
    /// </summary>
    public string? StatusIcon { get; set; }

    /// <summary>
    /// الدور الذي قام بتغيير الحالة (Customer, Store, Courier, System)
    /// </summary>
    public string ChangedByRole { get; set; } = string.Empty;

    /// <summary>
    /// الملاحظات والتبريرات إن وجدت
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// كائن مدخلات قبول الطلب من المتجر مع وقت التوصيل المتوقع
/// </summary>
public class AcceptOrderInput
{
    /// <summary>
    /// وقت التسليم والتوصيل المتوقع
    /// </summary>
    public DateTime? EstimatedDeliveryTime { get; set; }

    /// <summary>
    /// الوقت المتوقع بالدقائق (بديل اختياري لتسهيل الاختيار من واجهات التطبيق مثل: 30 أو 45 دقيقة)
    /// </summary>
    public int? EstimatedMinutes { get; set; }

    /// <summary>
    /// ملاحظات داخلية من المتجر
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// كائن مدخلات اعتماد وتأكيد دفع الطلب من المتجر
/// </summary>
public class ConfirmOrderPaymentInput
{
    /// <summary>
    /// ملاحظات تدقيق الإيصال البنكي أو المحفظة
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}
