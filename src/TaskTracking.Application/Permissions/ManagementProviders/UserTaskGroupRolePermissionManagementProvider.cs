using TaskTracking.Permissions.ValueProviders;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace TaskTracking.Permissions.ManagementProviders;

public class UserTaskGroupRolePermissionManagementProvider : PermissionManagementProvider
{
    public override string Name => UserTaskGroupRolePermissionValueProvider.ProviderName;

    public UserTaskGroupRolePermissionManagementProvider(IPermissionGrantRepository permissionGrantRepository,
        IGuidGenerator guidGenerator, ICurrentTenant currentTenant)
        : base(
            permissionGrantRepository,
            guidGenerator,
            currentTenant)
    {
    }
}