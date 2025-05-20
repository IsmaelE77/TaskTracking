using System;
using System.Collections.Generic;
using System.Text;
using TaskTracking.Localization;
using Volo.Abp.Application.Services;

namespace TaskTracking;

/* Inherit your application services from this class.
 */
public abstract class TaskTrackingAppService : ApplicationService
{
    protected TaskTrackingAppService()
    {
        LocalizationResource = typeof(TaskTrackingResource);
    }
}
