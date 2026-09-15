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

        // ── المنتجات ──
        var productsPermission = myGroup.AddPermission(
            TalabiPermissions.Products.Default, L("Permission:Products"));
        productsPermission.AddChild(TalabiPermissions.Products.Create, L("Permission:Products.Create"));
        productsPermission.AddChild(TalabiPermissions.Products.Edit,   L("Permission:Products.Edit"));
        productsPermission.AddChild(TalabiPermissions.Products.Delete, L("Permission:Products.Delete"));
        productsPermission.AddChild(TalabiPermissions.Products.Import, L("Permission:Products.Import"));

        // ── المتاجر ──
        var storesPermission = myGroup.AddPermission(
            TalabiPermissions.Stores.Default, L("Permission:Stores"));
        storesPermission.AddChild(TalabiPermissions.Stores.Create, L("Permission:Stores.Create"));
        storesPermission.AddChild(TalabiPermissions.Stores.Edit,   L("Permission:Stores.Edit"));
        storesPermission.AddChild(TalabiPermissions.Stores.Delete, L("Permission:Stores.Delete"));

        // ── أنواع المتاجر ──
        var storeTypesPermission = myGroup.AddPermission(
            TalabiPermissions.StoreTypes.Default, L("Permission:StoreTypes"));
        storeTypesPermission.AddChild(TalabiPermissions.StoreTypes.Create, L("Permission:StoreTypes.Create"));
        storeTypesPermission.AddChild(TalabiPermissions.StoreTypes.Edit,   L("Permission:StoreTypes.Edit"));
        storeTypesPermission.AddChild(TalabiPermissions.StoreTypes.Delete, L("Permission:StoreTypes.Delete"));

        // ── العملاء ──
        var customersPermission = myGroup.AddPermission(
            TalabiPermissions.Customers.Default, L("Permission:Customers"));
        customersPermission.AddChild(TalabiPermissions.Customers.Create, L("Permission:Customers.Create"));
        customersPermission.AddChild(TalabiPermissions.Customers.Edit,   L("Permission:Customers.Edit"));
        customersPermission.AddChild(TalabiPermissions.Customers.Delete, L("Permission:Customers.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TalabiResource>(name);
    }
}
