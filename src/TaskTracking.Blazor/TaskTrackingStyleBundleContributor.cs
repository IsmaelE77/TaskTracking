using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

namespace TaskTracking.Blazor;

public class TaskTrackingStyleBundleContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        context.Files.Remove("_content/Blazorise/blazorise.css");
        context.Files.Remove("_content/Blazorise.Bootstrap5/blazorise.bootstrap5.css");
        context.Files.Remove("_content/Blazorise.Snackbar/blazorise.snackbar.css");
        context.Files.Remove("_content/Volo.Abp.BlazoriseUI/volo.abp.blazoriseui.css");
    }
}