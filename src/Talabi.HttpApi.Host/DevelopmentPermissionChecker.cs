using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;

namespace Talabi;

/// <summary>
/// فئة لتخطي جميع الصلاحيات خلال مرحلة التطوير (ترجع True دائماً)
/// </summary>
[Dependency(ReplaceServices = true)]
public class DevelopmentPermissionChecker : IPermissionChecker, ITransientDependency
{
    public Task<bool> IsGrantedAsync(string name)
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsGrantedAsync(ClaimsPrincipal claimsPrincipal, string name)
    {
        return Task.FromResult(true);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(string[] names)
    {
        var result = new MultiplePermissionGrantResult();
        foreach (var name in names)
        {
            result.Result.Add(name, PermissionGrantResult.Granted);
        }
        return Task.FromResult(result);
    }

    public Task<MultiplePermissionGrantResult> IsGrantedAsync(ClaimsPrincipal claimsPrincipal, string[] names)
    {
        var result = new MultiplePermissionGrantResult();
        foreach (var name in names)
        {
            result.Result.Add(name, PermissionGrantResult.Granted);
        }
        return Task.FromResult(result);
    }
}
