using TaskTracking.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace TaskTracking.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class TaskTrackingPageModel : AbpPageModel
{
    protected TaskTrackingPageModel()
    {
        LocalizationResourceType = typeof(TaskTrackingResource);
    }
}
