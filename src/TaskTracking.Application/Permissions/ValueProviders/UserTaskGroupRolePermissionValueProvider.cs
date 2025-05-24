using System.Linq;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Users;

namespace TaskTracking.Permissions.ValueProviders;

public class UserTaskGroupRolePermissionValueProvider : PermissionValueProvider
{
    public const string ProviderName = "TaskGroupRolePermission";
    public override string Name => ProviderName;


    private readonly IUserTaskGroupRoleCacheService _userTaskGroupRoleCacheService;
    private readonly ICurrentUser _currentUser;
    private readonly ITaskGroupPermissionContext _permissionContext;

    public UserTaskGroupRolePermissionValueProvider(IPermissionStore permissionStore,
        IUserTaskGroupRoleCacheService userTaskGroupRoleCacheService, ICurrentUser currentUser,
        ITaskGroupPermissionContext permissionContext) : base(permissionStore)
    {
        _userTaskGroupRoleCacheService = userTaskGroupRoleCacheService;
        _currentUser = currentUser;
        _permissionContext = permissionContext;
    }

    public override async Task<PermissionGrantResult> CheckAsync(PermissionValueCheckContext context)
    {
        if (!_currentUser.IsAuthenticated ||
            !context.Permission.Name.StartsWith(UserTaskGroupPermissions.GroupName))
        {
            return PermissionGrantResult.Prohibited;
        }

        var hasPermission = await _permissionContext.HasPermissionAsync(context.Permission.Name);
        return hasPermission
            ? PermissionGrantResult.Granted
            : PermissionGrantResult.Prohibited;
    }


    public override async Task<MultiplePermissionGrantResult> CheckAsync(PermissionValuesCheckContext context)
    {
        var permissionNames = context.Permissions.Select(x => x.Name).Distinct().ToArray();

        if (!permissionNames.Any(x => x.StartsWith("TaskTracking.TaskGroups")))
        {
            return new MultiplePermissionGrantResult(permissionNames, PermissionGrantResult.Prohibited);
        }

        var results = true;

        foreach (var permissionName in permissionNames)
        {
            var hasPermission = await _permissionContext.HasPermissionAsync(permissionName);
            results &= hasPermission;
        }

        return new MultiplePermissionGrantResult(
            permissionNames,
            results ? PermissionGrantResult.Granted : PermissionGrantResult.Undefined
        );
    }
}