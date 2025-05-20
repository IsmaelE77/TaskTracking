using Volo.Abp.Modularity;

namespace TaskTracking;

[DependsOn(
    typeof(TaskTrackingDomainModule),
    typeof(TaskTrackingTestBaseModule)
)]
public class TaskTrackingDomainTestModule : AbpModule
{

}
