using Volo.Abp.Modularity;

namespace Talabi;

public abstract class TalabiApplicationTestBase<TStartupModule> : TalabiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
