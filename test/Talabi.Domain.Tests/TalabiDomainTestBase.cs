using Volo.Abp.Modularity;

namespace Talabi;

/* Inherit from this class for your domain layer tests. */
public abstract class TalabiDomainTestBase<TStartupModule> : TalabiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
