using System;
using System.Collections.Generic;
using System.Linq;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TaskTracking.TaskGroupAggregate.TaskItems;

public class TaskItem : FullAuditedEntity<Guid>, IHaveTaskGroup
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

        if (RecurrencePattern?.EndDate != null &&
            endDate.HasValue &&
            RecurrencePattern.EndDate.Value > endDate)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.RecurrenceEndDateExceedsTaskItemEndDate);
        }

        StartDate = startDate;
        EndDate = endDate;
    }


    internal void UpdateDetails(string title, string description, DateTime startDate, TaskType taskType, DateTime? endDate,
        RecurrencePattern? recurrencePattern)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), TaskItemConsts.MaxTitleLength);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), TaskItemConsts.MaxDescriptionLength);
        TaskType = taskType;
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

        if (recurrencePattern?.EndDate != null &&
            EndDate.HasValue &&
            recurrencePattern.EndDate.Value > EndDate
           )
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.RecurrenceEndDateExceedsTaskItemEndDate);
        }

        RecurrencePattern = recurrencePattern;
    }

    internal void RecordTaskProgress(
        Guid userId,
        DateOnly date)
    {
        var progress = UserProgresses
            .FirstOrDefault(up => up.UserId == userId);

        if (progress == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.ProgressNotFound);
        }

        var dueCount = GetDueCount();

        if(progress.ProgressEntries.Count == dueCount)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.AlreadyRecorded);
        }

        progress.RecordProgress(date);

        progress.SetProgressPercentage(progress.ProgressEntries.Count * 100 / dueCount);
    }


    public int GetDueCount()
    {
        if (RecurrencePattern == null)
        {
            return 1;
        }

        if (EndDate.HasValue)
        {
            return CountDueDatesInRange(StartDate, EndDate.Value);
        }

        if (RecurrencePattern.EndDate.HasValue)
        {
            return CountDueDatesInRange(StartDate, RecurrencePattern.EndDate.Value);
        }

        if (RecurrencePattern.Occurrences.HasValue)
        {
            switch (RecurrencePattern.RecurrenceType)
            {
                case RecurrenceType.Daily:
                    return RecurrencePattern.Occurrences.Value;

                case RecurrenceType.Weekly:
                    return RecurrencePattern.Occurrences.Value * RecurrencePattern.DaysOfWeek.Count;

                case RecurrenceType.Monthly:
                    return RecurrencePattern.Occurrences.Value;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        throw new BusinessException("Invalid recurrence pattern: must have EndDate or Occurrences.");
    }

    public bool IsDue(DateTime date)
    {
        if (UserProgresses.All(x => x.ProgressPercentage == 100))
        {
            return false;
        }

        if (TaskType == TaskType.OneTime )
        {
            return true;
        }

        return CountDueDatesInRange(date, date) > 0;
    }

    public int CountDueDatesInRange(DateTime from, DateTime to)
    {
        if (StartDate > to || (EndDate.HasValue && EndDate.Value < from))
            return 0;

        if (TaskType == TaskType.OneTime)
        {
            return StartDate >= from && StartDate <= to ? 1 : 0;
        }

        if (RecurrencePattern == null)
            return 0;

        var count = 0;
        var current = from;
        var endDate = RecurrencePattern.EndDate ?? EndDate ?? to;
        endDate = endDate > to ? to : endDate;

        switch (RecurrencePattern.RecurrenceType)
        {
            case RecurrenceType.Daily:
                while (current <= endDate)
                {
                    if (current >= from && current <= to)
                        count++;

                    if (RecurrencePattern.Occurrences.HasValue && count >= RecurrencePattern.Occurrences.Value)
                        break;

                    current = current.AddDays(RecurrencePattern.Interval);
                }

                break;

            case RecurrenceType.Weekly:
                var daysOfWeek = RecurrencePattern.DaysOfWeek;
                var weeks = 0;
                current = StartDate;

                while (current <= endDate)
                {
                    foreach (var day in daysOfWeek)
                    {
                        /*
                         * If StartDate = Monday, day = Wednesday, and Interval = 2,
                           then in the second recurrence week (weeks = 1),
                           we compute: StartDate + (1 * 7 * 2) + (Wed - Mon) = StartDate + 14 + 2 = +16 days.

                           This gives us the correct date (candidate) for that weekday in the recurrence week.
                         */
                        var candidate = StartDate.AddDays((weeks * 7 * RecurrencePattern.Interval) + (int)day -
                                                          (int)StartDate.DayOfWeek);
                        if (candidate < StartDate)
                            continue;

                        if (candidate >= from && candidate <= to && candidate <= endDate)
                        {
                            count++;
                            if (RecurrencePattern.Occurrences.HasValue && count >= RecurrencePattern.Occurrences.Value)
                                return count;
                        }
                    }

                    weeks++;
                    current = StartDate.AddDays(weeks * 7 * RecurrencePattern.Interval);
                }

                break;

            case RecurrenceType.Monthly:
                while (current <= endDate)
                {
                    if (current >= from && current <= to)
                        count++;

                    if (RecurrencePattern.Occurrences.HasValue && count >= RecurrencePattern.Occurrences.Value)
                        break;

                    current = current.AddMonths(RecurrencePattern.Interval);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return count;
    }
}