using Volo.Abp.Modularity;

namespace TaskTracking;

public abstract class TaskTrackingApplicationTestBase<TStartupModule> : TaskTrackingTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
