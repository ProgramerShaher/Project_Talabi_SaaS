using Talabi.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Talabi.Permissions;

/// <summary>
/// مزود تعريف صلاحيات منصة طلبي
/// </summary>
public class TalabiPermissionDefinitionProvider : PermissionDefinitionProvider
{
    /// <summary>
    /// تعريف جميع الصلاحيات وتسجيلها في نظام ABP
    /// </summary>
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(TalabiPermissions.GroupName, L("Permission:Talabi"));

        // ── التصنيفات العامة ──
        var categoriesPermission = myGroup.AddPermission(
            TalabiPermissions.Categories.Default, L("Permission:Categories"));
        categoriesPermission.AddChild(TalabiPermissions.Categories.Create, L("Permission:Categories.Create"));
        categoriesPermission.AddChild(TalabiPermissions.Categories.Edit,   L("Permission:Categories.Edit"));
        categoriesPermission.AddChild(TalabiPermissions.Categories.Delete, L("Permission:Categories.Delete"));
        categoriesPermission.AddChild(TalabiPermissions.Categories.Import, L("Permission:Categories.Import"));

        // ── تصنيفات المتاجر المخصصة ──
        var storeCategoriesPermission = myGroup.AddPermission(
            TalabiPermissions.StoreCategories.Default, L("Permission:StoreCategories"));
        storeCategoriesPermission.AddChild(TalabiPermissions.StoreCategories.Create, L("Permission:StoreCategories.Create"));
        storeCategoriesPermission.AddChild(TalabiPermissions.StoreCategories.Edit,   L("Permission:StoreCategories.Edit"));
        storeCategoriesPermission.AddChild(TalabiPermissions.StoreCategories.Delete, L("Permission:StoreCategories.Delete"));
        storeCategoriesPermission.AddChild(TalabiPermissions.StoreCategories.Import, L("Permission:StoreCategories.Import"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TalabiResource>(name);
    }
}
