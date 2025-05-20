using TaskTracking.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace TaskTracking.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class TaskTrackingController : AbpControllerBase
{
    protected TaskTrackingController()
    {
        LocalizationResource = typeof(TaskTrackingResource);
    }
}
