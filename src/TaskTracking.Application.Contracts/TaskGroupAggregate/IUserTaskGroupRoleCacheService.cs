using System;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;

namespace TaskTracking.TaskGroupAggregate;

public interface IUserTaskGroupRoleCacheService
{
    Task<UserTaskGroupWrap?> GetAsync();
    Task ClearAsync(Guid userId, Guid trackAccountId);
}