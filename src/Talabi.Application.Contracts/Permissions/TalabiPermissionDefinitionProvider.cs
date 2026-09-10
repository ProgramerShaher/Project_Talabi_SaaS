using Talabi.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Talabi.Permissions;

public class TalabiPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(TalabiPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(TalabiPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TalabiResource>(name);
    }
}
