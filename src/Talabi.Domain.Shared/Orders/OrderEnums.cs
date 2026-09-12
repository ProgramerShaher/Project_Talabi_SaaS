namespace Talabi.Orders;

/// <summary>
/// حالة دفع الطلب
/// </summary>
public enum OrderPaymentStatus
{
    /// <summary>
    /// قيد الانتظار
    /// </summary>
    Pending = 1,

    /// <summary>
    /// تم الدفع
    /// </summary>
    Paid = 2,

    /// <summary>
    /// فشلت عملية الدفع
    /// </summary>
    Failed = 3,

    /// <summary>
    /// تم استرجاع المبلغ
    /// </summary>
    Refunded = 4
}

/// <summary>
/// الفئة المستهدفة لسبب الإلغاء / الرفض
/// </summary>
public enum CancellationTargetAudience
{
    /// <summary>
    /// العميل
    /// </summary>
    Customer = 1,

    /// <summary>
    /// المتجر
    /// </summary>
    Store = 2,

    /// <summary>
    /// النظام / الإدارة
    /// </summary>
    System = 3
}
