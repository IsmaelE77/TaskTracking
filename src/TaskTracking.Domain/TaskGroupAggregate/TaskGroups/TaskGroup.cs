using System;
using System.Collections.Generic;
using System.Linq;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TaskTracking.TaskGroupAggregate.TaskGroups;

public class TaskGroup : FullAuditedAggregateRoot<Guid>, IAccessibleTaskGroup
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    #region Navigation Properties

    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();
    private readonly List<TaskItem> _tasks = new();

    public IReadOnlyCollection<UserTaskGroup> UserTaskGroups => _userTaskGroups.AsReadOnly();
    private readonly List<UserTaskGroup> _userTaskGroups = new();

    #endregion

    private TaskGroup()
    {
        // Required by EF Core
    }

    internal TaskGroup(
        Guid id,
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate) : base(id)
    {
        SetTitle(title);
        SetDescription(description);
        SetValidityPeriod(startDate, endDate);
    }

    internal void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), TaskGroupConsts.MaxTitleLength);
    }

    internal void SetDescription(string description)
    {
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), TaskGroupConsts.MaxDescriptionLength);
    }

    internal void SetValidityPeriod(DateTime startDate, DateTime? endDate)
    {
        if (endDate.HasValue && startDate > endDate.Value)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvalidDateRange);
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    internal void AddTask(TaskItem task)
    {
        if (task.EndDate.HasValue && EndDate.HasValue && task.EndDate.Value > EndDate.Value)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.TaskEndDateExceedsGroupEndDate);
        }

        _tasks.Add(task);
    }

    internal TaskItem UpdateTaskItem(
        Guid taskId,
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate,
        RecurrencePattern? recurrencePattern = null)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);

        if (task == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.TaskNotInGroup);
        }

        if (endDate.HasValue && EndDate.HasValue && endDate.Value > EndDate.Value)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.TaskEndDateExceedsGroupEndDate);
        }

        task.UpdateDetails(title, description, startDate, endDate, recurrencePattern);

        return task;
    }

    internal TaskItem DeleteTaskItem(
        Guid taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.TaskNotInGroup);
        }
        _tasks.Remove(task);
        return task;
    }

    internal void AddUserTaskGroup(UserTaskGroup userTaskGroup)
    {
        _userTaskGroups.Add(userTaskGroup);
    }

    internal UserTaskGroup RemoveUserTaskGroup(Guid userId)
    {
        var userTaskGroup = _userTaskGroups.FirstOrDefault(t => t.UserId == userId);

        if (userTaskGroup == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserNotInGroup);
        }

        if (userTaskGroup.Role == UserTaskGroupRole.Owner)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.CannotRemoveOwner);
        }

        _userTaskGroups.Remove(userTaskGroup);

        return userTaskGroup;
    }

    internal void ChangeUserPermission(Guid userId, UserTaskGroupRole newRole)
    {
        var userTaskGroup = _userTaskGroups.FirstOrDefault(utg => utg.UserId == userId);
        if (userTaskGroup == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserNotInGroup);
        }

        if (userTaskGroup.Role == UserTaskGroupRole.Owner)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.CannotChangeOwnerRole);
        }

        userTaskGroup.ChangeRole(newRole);
    }

    internal UserTaskProgress CreateUserTaskProgress(
        Guid id,
        Guid taskItemId,
        Guid userId,
        int progressPercentage = 0)
    {
        var taskItem = _tasks.FirstOrDefault(t => t.Id == taskItemId);
        if (taskItem == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.TaskNotInGroup);
        }

        var userTaskGroup = _userTaskGroups.FirstOrDefault(utg => utg.UserId == userId);
        if (userTaskGroup == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserNotInGroup);
        }

        var existingProgress = taskItem.UserProgresses
            .FirstOrDefault(up => up.UserId == userId );

        if (existingProgress != null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.ProgressAlreadyExists);
        }

        var progress = new UserTaskProgress(
            id,
            userId,
            taskItemId,
            progressPercentage);

        taskItem.AddUserProgress(progress);

        return progress;
    }

    internal void RecordTaskProgress(Guid taskItemId, Guid userId, DateOnly date)
    {
        var taskItem = _tasks.FirstOrDefault(t => t.Id == taskItemId);
        if (taskItem == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.TaskNotInGroup);
        }

        var userTaskGroup = _userTaskGroups.FirstOrDefault(utg => utg.UserId == userId);
        if (userTaskGroup == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserNotInGroup);
        }

        taskItem.RecordTaskProgress(userId,date);
    }

    private UserTaskProgress GetUserTaskProgress(Guid taskItemId, Guid userId)
    {
        var taskItem = _tasks.FirstOrDefault(t => t.Id == taskItemId);
        if (taskItem == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.TaskNotInGroup);
        }

        var userTaskGroup = _userTaskGroups.FirstOrDefault(utg => utg.UserId == userId);
        if (userTaskGroup == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserNotInGroup);
        }

        var progress = taskItem.UserProgresses
            .FirstOrDefault(up => up.UserId == userId);

        if (progress == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.ProgressNotFound);
        }

        return progress;
    }

}