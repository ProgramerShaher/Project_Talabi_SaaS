using Talabi.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Talabi.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(TalabiEntityFrameworkCoreModule),
    typeof(TalabiApplicationContractsModule)
    )]
public class TalabiDbMigratorModule : AbpModule
{
}
