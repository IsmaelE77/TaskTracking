using System;
using System.Collections.Generic;
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
        RecurrencePattern recurrencePattern = null);

    Task<TaskItem> UpdateTaskItemAsync(
        Guid taskGroupId,
        Guid taskItemId,
        string title,
        string description,
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

    Task<UserTaskProgress> UpdateProgressAsync(
        Guid taskGroupId,
        Guid taskItemId,
        Guid userId,
        int progressPercentage,
        string notes);

    Task<UserTaskProgress> CreateProgressAsync(
        Guid taskGroupId,
        Guid taskItemId,
        Guid userId,
        int progressPercentage = 0,
        string notes = "");

    Task<UserTaskProgress> MarkProgressAsCompletedAsync(
        Guid taskGroupId,
        Guid taskItemId,
        Guid userId);

    Task<UserTaskProgress> MarkProgressAsIncompletedAsync(
        Guid taskGroupId,
        Guid taskItemId,
        Guid userId);

    Task<TaskGroup> GetWithDetailsAsync(Guid taskGroupId);

    Task<TaskGroup> GetWithDetailsWithItemTaskDetailsAsync(Guid taskGroupId);

    Task<List<TaskGroup>> GetUserTaskGroupsAsync(Guid userId);

    Task<List<TaskGroup>> GetUserActiveTaskGroupsAsync(Guid userId);

    Task<List<TaskGroup>> GetUserOwnedTaskGroupsAsync(Guid userId);
}