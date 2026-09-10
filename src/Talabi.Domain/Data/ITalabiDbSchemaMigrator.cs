using System.Threading.Tasks;

namespace Talabi.Data;

public interface ITalabiDbSchemaMigrator
{
    Task MigrateAsync();
}
