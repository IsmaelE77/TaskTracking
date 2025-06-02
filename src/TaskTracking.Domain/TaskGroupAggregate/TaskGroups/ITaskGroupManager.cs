using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using Volo.Abp.Domain.Services;

namespace TaskTracking.TaskGroupAggregate.TaskGroups;

public interface ITaskGroupManager : IDomainService
{
    Task<TaskGroup> CreateAsync(
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate,
        Guid creatorUserId);

    Task<TaskGroup> UpdateAsync(
        Guid id,
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate);

    Task<Guid> DeleteAsync(Guid id);

    Task<TaskItem> CreateTaskItemAsync(
        Guid taskGroupId,
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate,
        TaskType taskType,
        RecurrencePattern? recurrencePattern = null);

    Task<TaskItem> UpdateTaskItemAsync(
        Guid taskGroupId,
        Guid taskItemId,
        string title,
        string description,
        TaskType taskType,
        DateTime startDate,
        DateTime? endDate,
        RecurrencePattern? recurrencePattern = null);

    Task<TaskItem> DeleteTaskItemAsync(
        Guid taskGroupId,
        Guid taskItemId);

    Task<UserTaskGroup> AddUserToGroupAsync(
        Guid taskGroupId,
        Guid userId,
        UserTaskGroupRole role);

    Task RemoveUserFromGroupAsync(Guid taskGroupId, Guid userId);

    Task ChangeUserGroupPermissionAsync(Guid taskGroupId, Guid userId, UserTaskGroupRole newRole);

    Task RecordTaskProgressAsync(
        Guid taskGroupId,
        Guid taskItemId,
        Guid userId,
        DateOnly date);

    Task RemoveTaskProgressAsync(
        Guid taskGroupId,
        Guid taskItemId,
        Guid userId,
        DateOnly date);

    Task<TaskGroup> GetWithDetailsAsync(Guid taskGroupId);

    Task<(List<TaskGroup> Items, int TotalCount)> GetUserTaskGroupsAsync(Guid userId, int skipCount, int maxResultCount);

    Task<(List<TaskGroup> Items, int TotalCount)> GetUserActiveTaskGroupsAsync(Guid userId, int skipCount, int maxResultCount);

    Task<(List<TaskGroup> Items, int TotalCount)> GetUserOwnedTaskGroupsAsync(Guid userId, int skipCount, int maxResultCount);
}