using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace TaskTracking.Permissions.Services;

public class TaskGroupPermissionContext : ITaskGroupPermissionContext, ITransientDependency

{
    private readonly ICurrentUser _currentUser;
    private readonly IUserTaskGroupRoleCacheService _roleCacheService;
    private readonly TaskGroupPermissionEvaluator _permissionEvaluator;

    public TaskGroupPermissionContext(
        ICurrentUser currentUser,
        IUserTaskGroupRoleCacheService roleCacheService,
        TaskGroupPermissionEvaluator permissionEvaluator)
    {
        _currentUser = currentUser;
        _roleCacheService = roleCacheService;
        _permissionEvaluator = permissionEvaluator;
    }

    public async Task<bool> HasPermissionAsync(string permissionName)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        var roleWrap = await _roleCacheService.GetAsync();
        if (roleWrap == null)
            return false;

        return _permissionEvaluator.IsPermissionGrantedForRole(roleWrap.Role, permissionName);
    }

    public async Task<UserTaskGroupRole?> GetCurrentUserRoleAsync()
    {
        var roleWrap = await _roleCacheService.GetAsync();
        return roleWrap?.Role;
    }

}