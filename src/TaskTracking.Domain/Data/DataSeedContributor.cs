using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace TaskTracking.Data;

public class DataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly TaskTrackingDataSeedContributor _taskTrackingDataSeedContributor;

    public DataSeedContributor(TaskTrackingDataSeedContributor taskTrackingDataSeedContributor)
    {
        _taskTrackingDataSeedContributor = taskTrackingDataSeedContributor;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await _taskTrackingDataSeedContributor.SeedAsync(context);
    }
}