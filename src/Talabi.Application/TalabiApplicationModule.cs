using Volo.Abp.Account;
using Volo.Abp.Mapperly;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Talabi;

[DependsOn(
    typeof(TalabiDomainModule),
    typeof(TalabiApplicationContractsModule),
    typeof(AbpFluentValidationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class TalabiApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<TalabiApplicationModule>();
    }
}
