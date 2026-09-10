using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Talabi.Data;

/* This is used if database provider does't define
 * ITalabiDbSchemaMigrator implementation.
 */
public class NullTalabiDbSchemaMigrator : ITalabiDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
