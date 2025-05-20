using TaskTracking.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace TaskTracking.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(TaskTrackingEntityFrameworkCoreModule),
    typeof(TaskTrackingApplicationContractsModule)
    )]
public class TaskTrackingDbMigratorModule : AbpModule
{
}
