using TaskTracking.Samples;
using Xunit;

namespace TaskTracking.EntityFrameworkCore.Applications;

[Collection(TaskTrackingTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<TaskTrackingEntityFrameworkCoreTestModule>
{

}
