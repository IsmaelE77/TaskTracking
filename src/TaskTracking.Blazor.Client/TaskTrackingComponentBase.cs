using TaskTracking.Localization;
using Volo.Abp.AspNetCore.Components;

namespace TaskTracking.Blazor.Client;

public abstract class TaskTrackingComponentBase : AbpComponentBase
{
    protected TaskTrackingComponentBase()
    {
        LocalizationResource = typeof(TaskTrackingResource);
    }
}
