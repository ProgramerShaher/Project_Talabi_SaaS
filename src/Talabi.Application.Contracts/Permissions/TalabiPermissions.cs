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
    /// صلاحيات إدارة المتاجر
    /// </summary>
    public static class Stores
    {
        public const string Default = GroupName + ".Stores";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
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
}
