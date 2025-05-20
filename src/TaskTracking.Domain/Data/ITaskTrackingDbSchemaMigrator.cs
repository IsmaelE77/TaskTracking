using System.Threading.Tasks;

namespace TaskTracking.Data;

public interface ITaskTrackingDbSchemaMigrator
{
    Task MigrateAsync();
}
