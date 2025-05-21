using System;
using System.Collections.Generic;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TaskTracking.TaskGroupAggregate.TaskItems;

public class TaskItem : FullAuditedEntity<Guid>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public TaskType TaskType { get; private set; }
    public RecurrencePattern? RecurrencePattern { get; private set; }
    public Guid TaskGroupId { get; private set; }
    public TaskGroup TaskGroup { get; private set; }

    #region Navigation Properties

    public IReadOnlyCollection<UserTaskProgress> UserProgresses => _userProgresses.AsReadOnly();
    private readonly List<UserTaskProgress> _userProgresses = new();

    #endregion

    private TaskItem()
    {
        // Required by EF Core
    }

    internal static TaskItem CreateOneTimeTask(
        Guid id,
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate,
        Guid taskGroupId)
    {
        return new TaskItem(
            id,
            title,
            description,
            startDate,
            endDate,
            TaskType.OneTime,
            null,
            taskGroupId);
    }

    internal static TaskItem CreateRecurringTask(
        Guid id,
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate,
        RecurrencePattern recurrencePattern,
        Guid taskGroupId)
    {
        var task = new TaskItem(
            id,
            title,
            description,
            startDate,
            endDate,
            TaskType.Recurring,
            recurrencePattern,
            taskGroupId);

        return task;
    }

    private TaskItem(
        Guid id,
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate,
        TaskType taskType,
        RecurrencePattern recurrencePattern,
        Guid taskGroupId) : base(id)
    {
        SetTitle(title);
        SetDescription(description);
        SetDateRange(startDate, endDate);
        TaskType = taskType;
        SetRecurrencePattern(recurrencePattern);
        TaskGroupId = taskGroupId;
    }

    internal void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), TaskItemConsts.MaxTitleLength);
    }

    internal void SetDescription(string description)
    {
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), TaskItemConsts.MaxDescriptionLength);
    }

    internal void SetDateRange(DateTime startDate, DateTime? endDate)
    {
        if (endDate.HasValue && startDate > endDate.Value)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvalidDateRange);
        }

        StartDate = startDate;
        EndDate = endDate;
    }


    internal void UpdateDetails(string title, string description, DateTime startDate, DateTime? endDate,
        RecurrencePattern? recurrencePattern)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), TaskItemConsts.MaxTitleLength);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), TaskItemConsts.MaxDescriptionLength);
        SetDateRange(startDate, endDate);
        SetRecurrencePattern(recurrencePattern);
    }

    internal void AddUserProgress(UserTaskProgress progress)
    {
        _userProgresses.Add(progress);
    }

    internal void SetRecurrencePattern(RecurrencePattern? recurrencePattern)
    {
        if (recurrencePattern is not null && TaskType != TaskType.Recurring)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.CannotSetRecurrencePatternForOneTimeTask);
        }

        RecurrencePattern = recurrencePattern;
    }
}