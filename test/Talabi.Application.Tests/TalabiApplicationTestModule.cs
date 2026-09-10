using Volo.Abp.Modularity;

namespace Talabi;

[DependsOn(
    typeof(TalabiApplicationModule),
    typeof(TalabiDomainTestModule)
)]
public class TalabiApplicationTestModule : AbpModule
{

}
