namespace Talabi.Deliveries;

/// <summary>
/// حالة تعيين التوصيل
/// </summary>
public enum DeliveryAssignmentStatus
{
    /// <summary>
    /// تم التعيين للمندوب
    /// </summary>
    Assigned = 1,

    /// <summary>
    /// قبل المندوب الطلب
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// تم استلام الطلب من المتجر
    /// </summary>
    PickedUp = 3,

    /// <summary>
    /// في الطريق إلى العميل
    /// </summary>
    InTransit = 4,

    /// <summary>
    /// تم التسليم للعميل
    /// </summary>
    Delivered = 5,

    /// <summary>
    /// فشل التوصيل
    /// </summary>
    Failed = 6
}

/// <summary>
/// الجهة التي قامت بتأكيد استلام الطلب
/// </summary>
public enum DeliveryConfirmedBy
{
    /// <summary>
    /// العميل
    /// </summary>
    Customer = 1,

    /// <summary>
    /// المندوب
    /// </summary>
    Courier = 2,

    /// <summary>
    /// النظام تلقائياً
    /// </summary>
    System = 3
}
