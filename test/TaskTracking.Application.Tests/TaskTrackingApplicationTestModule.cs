using Volo.Abp.Modularity;

namespace TaskTracking;

[DependsOn(
    typeof(TaskTrackingApplicationModule),
    typeof(TaskTrackingDomainTestModule)
)]
public class TaskTrackingApplicationTestModule : AbpModule
{

}
