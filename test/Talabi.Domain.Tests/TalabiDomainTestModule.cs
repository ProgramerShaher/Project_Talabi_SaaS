using Volo.Abp.Modularity;

namespace Talabi;

[DependsOn(
    typeof(TalabiDomainModule),
    typeof(TalabiTestBaseModule)
)]
public class TalabiDomainTestModule : AbpModule
{

}
