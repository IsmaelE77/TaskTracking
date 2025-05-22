using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TaskTracking.TaskGroupAggregate.TaskItems;

public class TaskItemManager : DomainService
{
    private readonly IReadOnlyRepository<TaskItem, Guid> _taskItemRepository;
    private readonly IReadOnlyRepository<TaskGroup, Guid> _taskGroupRepository;
    private readonly IReadOnlyRepository<UserTaskGroup, Guid> _userTaskGroupRepository;
    private readonly IReadOnlyRepository<UserTaskProgress, Guid> _userTaskProgressRepository;

    public TaskItemManager(
        IReadOnlyRepository<TaskItem, Guid> taskItemRepository,
        IReadOnlyRepository<TaskGroup, Guid> taskGroupRepository,
        IReadOnlyRepository<UserTaskGroup, Guid> userTaskGroupRepository,
        IReadOnlyRepository<UserTaskProgress, Guid> userTaskProgressRepository)
    {
        _taskItemRepository = taskItemRepository;
        _taskGroupRepository = taskGroupRepository;
        _userTaskGroupRepository = userTaskGroupRepository;
        _userTaskProgressRepository = userTaskProgressRepository;
    }

    public async Task<(List<TaskItem> Items, int TotalCount)> GetTasksForTodayPagedAsync(
        Guid userId,
        int skipCount,
        int maxResultCount)
    {
        var today = Clock.Now.Date;
        return await GetTasksForDateRangePagedAsync(userId, skipCount, maxResultCount, today, today);
    }

    public async Task<(List<TaskItem> Items, int TotalCount)> GetTasksForNextNDaysPagedAsync(
        Guid userId,
        int days,
        int skipCount,
        int maxResultCount)
    {
        var today = Clock.Now.Date;
        var endDate = today.AddDays(days);
        return await GetTasksForDateRangePagedAsync(userId, skipCount, maxResultCount, today, endDate);
    }

    private async Task<(List<TaskItem> Items, int TotalCount)> GetTasksForDateRangePagedAsync(
        Guid userId,
        int skipCount,
        int maxResultCount,
        DateTime startDate,
        DateTime endDate)
    {
        var query = await _taskItemRepository.GetQueryableAsync();
        var userTaskGroups = await _userTaskGroupRepository.GetQueryableAsync();

        // Calculate bitmask for days of week in the range
        int daysOfWeekInRangeFlags = 0;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            daysOfWeekInRangeFlags |= (1 << (int)date.DayOfWeek);
        }

        // Build base query for active tasks in the period
        var baseQuery = from task in query
            join userGroup in userTaskGroups on task.TaskGroupId equals userGroup.TaskGroupId
            where userGroup.UserId == userId &&
                  (
                      // One-time tasks within the date range
                      (task.TaskType == TaskType.OneTime &&
                       task.StartDate.Date >= startDate &&
                       task.StartDate.Date <= endDate) ||

                      // Daily recurring tasks active in the period
                      (task.TaskType == TaskType.Recurring &&
                       task.RecurrencePattern.RecurrenceType == RecurrenceType.Daily &&
                       task.StartDate.Date <= endDate &&
                       (task.EndDate == null || task.EndDate.Value.Date >= startDate) &&
                       (task.RecurrencePattern.EndDate == null || task.RecurrencePattern.EndDate.Value.Date >= startDate)) ||

                      // Weekly recurring tasks with at least one occurrence in the period
                      (task.TaskType == TaskType.Recurring &&
                       task.RecurrencePattern.RecurrenceType == RecurrenceType.Weekly &&
                       task.StartDate.Date <= endDate &&
                       (task.EndDate == null || task.EndDate.Value.Date >= startDate) &&
                       (task.RecurrencePattern.EndDate == null || task.RecurrencePattern.EndDate.Value.Date >= startDate) &&
                       (task.RecurrencePattern.DaysOfWeekFlags & daysOfWeekInRangeFlags) != 0) ||

                      // Monthly recurring tasks active in the period
                      // We'll refine these further in memory
                      (task.TaskType == TaskType.Recurring &&
                       task.RecurrencePattern.RecurrenceType == RecurrenceType.Monthly &&
                       task.StartDate.Date <= endDate &&
                       (task.EndDate == null || task.EndDate.Value.Date >= startDate) &&
                       (task.RecurrencePattern.EndDate == null || task.RecurrencePattern.EndDate.Value.Date >= startDate))
                  )
            select task;

        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(baseQuery);

        // Apply paging
        var pagedQuery = baseQuery.OrderBy(t => t.StartDate)
                                 .Skip(skipCount)
                                 .Take(maxResultCount);

        var items = await AsyncExecuter.ToListAsync(pagedQuery);

        // For monthly tasks, we need to filter them further
        var result = items.Where(task =>
            task.TaskType != TaskType.Recurring ||
            task.RecurrencePattern!.RecurrenceType != RecurrenceType.Monthly ||
            DoesMonthlyTaskOccurInRange(task, startDate, endDate)
        ).ToList();

        return (result, totalCount);
    }

    private bool DoesMonthlyTaskOccurInRange(TaskItem task, DateTime startDate, DateTime endDate)
    {
        if (task.TaskType != TaskType.Recurring || task.RecurrencePattern!.RecurrenceType != RecurrenceType.Monthly)
            return false;

        var dayOfMonth = task.StartDate.Day;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (date.Day == dayOfMonth)
                return true;
        }

        return false;
    }

}