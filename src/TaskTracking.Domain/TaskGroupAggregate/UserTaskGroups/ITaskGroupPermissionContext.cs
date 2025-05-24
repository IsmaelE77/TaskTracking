using System.Threading.Tasks;

namespace TaskTracking.TaskGroupAggregate.UserTaskGroups;

public interface ITaskGroupPermissionContext
{
    Task<bool> HasPermissionAsync(string permissionName);
    Task<UserTaskGroupRole?> GetCurrentUserRoleAsync();
}