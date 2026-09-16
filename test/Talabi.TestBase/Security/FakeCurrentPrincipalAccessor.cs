using System;
using System.Collections.Generic;
using System.Security.Claims;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace Talabi.Security;

[Dependency(ReplaceServices = true)]
public class FakeCurrentPrincipalAccessor : ThreadCurrentPrincipalAccessor
{
    private ClaimsPrincipal? _principal;

    public override IDisposable Change(ClaimsPrincipal principal)
    {
        var parent = _principal;
        _principal = principal;
        return new DisposeAction(() => _principal = parent);
    }

    protected override ClaimsPrincipal GetClaimsPrincipal()
    {
        return _principal ?? GetPrincipal();
    }

    private ClaimsPrincipal GetPrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, "2e701e62-0953-4dd3-910b-dc6cc93ccb0d"),
            new Claim(AbpClaimTypes.UserName, "admin"),
            new Claim(AbpClaimTypes.Email, "admin@abp.io")
        }));
    }
}
