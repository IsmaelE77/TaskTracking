using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace TaskTracking.EntityFrameworkCore;

[DependsOn(
    typeof(TaskTrackingDomainModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule)
    )]
public class TaskTrackingEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        TaskTrackingEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<TaskTrackingDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
                /* The main point to change your DBMS.
                 * See also TaskTrackingMigrationsDbContextFactory for EF Core tooling. */
            options.UseNpgsql();
        });

        Configure<AbpEntityOptions>(options =>
        {
            options.Entity<TaskGroup>(orderOptions =>
            {
                orderOptions.DefaultWithDetailsFunc = query => query
                    .Include(tg => tg.UserTaskGroups)
                    .Include(tg => tg.Tasks)
                    .ThenInclude(t => t.UserProgresses);
            });
        });

    }
}