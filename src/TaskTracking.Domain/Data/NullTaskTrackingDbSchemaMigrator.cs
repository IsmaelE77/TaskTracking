using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TaskTracking.Data;

/* This is used if database provider does't define
 * ITaskTrackingDbSchemaMigrator implementation.
 */
public class NullTaskTrackingDbSchemaMigrator : ITaskTrackingDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
