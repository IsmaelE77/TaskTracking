using Xunit;

namespace TaskTracking.EntityFrameworkCore;

[CollectionDefinition(TaskTrackingTestConsts.CollectionDefinitionName)]
public class TaskTrackingEntityFrameworkCoreCollection : ICollectionFixture<TaskTrackingEntityFrameworkCoreFixture>
{

}
