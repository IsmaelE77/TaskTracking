using TaskTracking.Samples;
using Xunit;

namespace TaskTracking.EntityFrameworkCore.Domains;

[Collection(TaskTrackingTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<TaskTrackingEntityFrameworkCoreTestModule>
{

}
