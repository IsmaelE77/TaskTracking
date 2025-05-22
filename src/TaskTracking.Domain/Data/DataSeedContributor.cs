using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace TaskTracking.Data;

public class DataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly TaskTrackingDataSeedContributor _taskTrackingDataSeedContributor;
    private readonly IDataFilter _dataFilter;

    public DataSeedContributor(TaskTrackingDataSeedContributor taskTrackingDataSeedContributor, IDataFilter dataFilter)
    {
        _taskTrackingDataSeedContributor = taskTrackingDataSeedContributor;
        _dataFilter = dataFilter;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using var _ = _dataFilter.Disable<IAccessibleTaskGroup>();
        using var __ = _dataFilter.Disable<IHaveTaskGroup>();

        await _taskTrackingDataSeedContributor.SeedAsync(context);
    }
}