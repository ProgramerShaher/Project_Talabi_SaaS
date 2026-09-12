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
}
