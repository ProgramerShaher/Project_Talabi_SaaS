namespace Talabi.Orders;

/// <summary>
/// ثوابت وقيود حقول الطلبات
/// </summary>
public static class OrderConsts
{
    public const int MaxOrderNumberLength = 20;
    public const int MaxCustomerNotesLength = 1000;
    public const int MaxStoreNotesLength = 1000;
}

/// <summary>
/// ثوابت وقيود حقول عناصر الطلب
/// </summary>
public static class OrderItemConsts
{
    public const int MaxProductNameLength = 200;
    public const int MaxProductImageUrlLength = 500;
    public const int MaxSkuLength = 50;
    public const int MaxUnitLength = 50;
    public const int MaxNotesLength = 500;
}

/// <summary>
/// ثوابت وقيود حقول حالات الطلب
/// </summary>
public static class OrderStatusConsts
{
    public const int MaxNameLength = 50;
    public const int MaxDisplayNameLength = 100;
    public const int MaxColorLength = 20;
    public const int MaxIconLength = 50;
}

/// <summary>
/// ثوابت وقيود حقول سجل حالات الطلب
/// </summary>
public static class OrderStatusHistoryConsts
{
    public const int MaxChangedByRoleLength = 50;
    public const int MaxNotesLength = 500;
}

/// <summary>
/// ثوابت وقيود أسباب الرفض والإلغاء
/// </summary>
public static class CancellationReasonConsts
{
    public const int MaxReasonLength = 200;
    public const int MaxAdditionalNotesLength = 500;
    public const int MaxCancelledByRoleLength = 50;
}
