using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracking.Data;
using Volo.Abp.DependencyInjection;

namespace TaskTracking.EntityFrameworkCore;

public class EntityFrameworkCoreTaskTrackingDbSchemaMigrator
    : ITaskTrackingDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreTaskTrackingDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the TaskTrackingDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<TaskTrackingDbContext>()
            .Database
            .MigrateAsync();
    }
}
