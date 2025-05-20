using Microsoft.Extensions.Localization;
using TaskTracking.Localization;
using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace TaskTracking.Web;

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
