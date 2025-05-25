using Microsoft.Extensions.Localization;
using TaskTracking.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace TaskTracking.Blazor.Client;

[Dependency(ReplaceServices = true)]
public class TaskTrackingBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<TaskTrackingResource> _localizer;

    public TaskTrackingBrandingProvider(IStringLocalizer<TaskTrackingResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
