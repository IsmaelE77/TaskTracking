using System.Collections.Generic;
using Volo.Abp.DependencyInjection;

namespace TaskTracking.TaskGroupAggregate.UserTaskGroups;

public class TaskGroupPermissionEvaluator : ITransientDependency
{
    private readonly Dictionary<UserTaskGroupRole, HashSet<string>> _rolePermissions;

    public TaskGroupPermissionEvaluator()
    {
        _rolePermissions = new Dictionary<UserTaskGroupRole, HashSet<string>>
        {
            {
                UserTaskGroupRole.Owner,
                new HashSet<string>
                {
                    UserTaskGroupPermissions.Update,
                    UserTaskGroupPermissions.Delete,
                    UserTaskGroupPermissions.ManageUsers,
                    UserTaskGroupPermissions.RecordProgress,
                    UserTaskGroupPermissions.ManageTaskItems,
                    UserTaskGroupPermissions.CreateTaskItems,
                    UserTaskGroupPermissions.UpdateTaskItems,
                    UserTaskGroupPermissions.DeleteTaskItems,
                    UserTaskGroupPermissions.GenerateInvitations,
                }
            },
            {
                UserTaskGroupRole.CoOwner,
                new HashSet<string>
                {
                    UserTaskGroupPermissions.Update,
                    UserTaskGroupPermissions.Delete,
                    UserTaskGroupPermissions.RecordProgress,
                    UserTaskGroupPermissions.ManageTaskItems,
                    UserTaskGroupPermissions.CreateTaskItems,
                    UserTaskGroupPermissions.UpdateTaskItems,
                    UserTaskGroupPermissions.DeleteTaskItems,
                }
            },
            {
                UserTaskGroupRole.Subscriber,
                new HashSet<string>
                {
                    UserTaskGroupPermissions.RecordProgress,
                }
            },
        };
    }

    public bool IsPermissionGrantedForRole(UserTaskGroupRole role, string permission)
    {
        return _rolePermissions.TryGetValue(role, out var permissions) &&
               permissions.Contains(permission);
    }
}