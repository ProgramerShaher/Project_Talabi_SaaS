namespace Talabi.Permissions;

/// <summary>
/// تعريف جميع صلاحيات منصة طلبي
/// </summary>
public static class TalabiPermissions
{
    /// <summary>
    /// اسم مجموعة الصلاحيات الرئيسية
    /// </summary>
    public const string GroupName = "Talabi";

    /// <summary>
    /// صلاحيات إدارة التصنيفات العامة للنظام (يديرها مدير النظام)
    /// </summary>
    public static class Categories
    {
        public const string Default = GroupName + ".Categories";
        public const string Create = Default + ".Create";
        public const string Edit   = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Import = Default + ".Import";
    }

    /// <summary>
    /// صلاحيات إدارة تصنيفات المتاجر المخصصة (يديرها صاحب المتجر)
    /// </summary>
    public static class StoreCategories
    {
        public const string Default = GroupName + ".StoreCategories";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
        public const string Import  = Default + ".Import";
    }

    /// <summary>
    /// صلاحيات إدارة المنتجات
    /// </summary>
    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
        public const string Import  = Default + ".Import";
    }

    /// <summary>
    /// صلاحيات إدارة وحدات البيع
    /// </summary>
    public static class SalesUnits
    {
        public const string Default = GroupName + ".SalesUnits";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    /// <summary>
    /// صلاحيات إدارة المتاجر
    /// </summary>
    public static class Stores
    {
        public const string Default = GroupName + ".Stores";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
        public const string Approve = Default + ".Approve";
        public const string Suspend = Default + ".Suspend";
        public const string ManageWorkingHours = Default + ".ManageWorkingHours";
    }

    /// <summary>
    /// صلاحيات إدارة أنواع المتاجر
    /// </summary>
    public static class StoreTypes
    {
        public const string Default = GroupName + ".StoreTypes";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    /// <summary>
    /// صلاحيات إدارة العملاء
    /// </summary>
    public static class Customers
    {
        public const string Default = GroupName + ".Customers";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    /// <summary>
    /// صلاحيات إدارة تقييمات ومراجعات المتاجر
    /// </summary>
    public static class Reviews
    {
        public const string Default = GroupName + ".Reviews";
        public const string Reply   = Default + ".Reply";
        public const string Manage  = Default + ".Manage";
    }

    /// <summary>
    /// صلاحيات إدارة المناديب والتوصيل الميداني
    /// </summary>
    public static class Deliveries
    {
        public const string Default        = GroupName + ".Deliveries";
        public const string ManageCouriers = Default + ".ManageCouriers";
        public const string Assign         = Default + ".Assign";
        public const string Track          = Default + ".Track";
    }

    /// <summary>
    /// صلاحيات إدارة المدفوعات والتحقق من إيصالات التحويل المالي
    /// </summary>
    public static class Payments
    {
        public const string Default        = GroupName + ".Payments";
        public const string VerifyReceipts = Default + ".VerifyReceipts";
        public const string Manage         = Default + ".Manage";
    }
}

