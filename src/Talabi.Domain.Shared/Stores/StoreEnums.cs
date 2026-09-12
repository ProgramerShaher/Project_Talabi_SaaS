namespace Talabi.Stores;

/// <summary>
/// حالة المتجر
/// </summary>
public enum StoreStatus
{
    /// <summary>
    /// نشط
    /// </summary>
    Active = 1,

    /// <summary>
    /// غير نشط
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// موقوف مؤقتاً
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// بانتظار الموافقة والاعتماد
    /// </summary>
    PendingApproval = 4
}

/// <summary>
/// دور مستخدم / موظف المتجر
/// </summary>
public enum StoreUserRole
{
    /// <summary>
    /// مالك المتجر
    /// </summary>
    Owner = 1,

    /// <summary>
    /// مدير المتجر
    /// </summary>
    Manager = 2,

    /// <summary>
    /// موظف عادي
    /// </summary>
    Employee = 3
}
