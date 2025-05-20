using Volo.Abp.Modularity;

namespace TaskTracking;

/* Inherit from this class for your domain layer tests. */
public abstract class TaskTrackingDomainTestBase<TStartupModule> : TaskTrackingTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
