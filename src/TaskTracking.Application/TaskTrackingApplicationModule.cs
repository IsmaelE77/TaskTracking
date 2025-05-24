using TaskTracking.Permissions.ManagementProviders;
using TaskTracking.Permissions.ValueProviders;
using Volo.Abp.Account;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace TaskTracking;

[DependsOn(
    typeof(TaskTrackingDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(TaskTrackingApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class TaskTrackingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<TaskTrackingApplicationModule>();
        });

        Configure<PermissionManagementOptions>(options =>
        {
            options.ManagementProviders.Add<UserTaskGroupRolePermissionManagementProvider>();
        });


        Configure<AbpPermissionOptions>(options =>
        {
            options.ValueProviders.Add<UserTaskGroupRolePermissionValueProvider>();
        });
    }
}