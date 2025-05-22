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

    public async Task<List<TaskItem>> GetTasksForTodayAsync(Guid userId)
    {
        var query = await _taskItemRepository.GetQueryableAsync();
        var userTaskGroups = await _userTaskGroupRepository.GetQueryableAsync();

        var today = Clock.Now.Date;

        // Get the base query without the problematic condition
        var baseQuery = from task in query
            join userGroup in userTaskGroups on task.TaskGroupId equals userGroup.TaskGroupId
            where userGroup.UserId == userId &&
                  task.StartDate.Date <= today &&
                  (task.EndDate == null || task.EndDate.Value.Date >= today) &&
                  (
                      // One-time tasks for today
                      (task.TaskType == TaskType.OneTime && task.StartDate.Date == today) ||
                      // Recurring daily tasks
                      (task.TaskType == TaskType.Recurring &&
                       task.RecurrencePattern.RecurrenceType == RecurrenceType.Daily) ||
                      // Recurring monthly tasks on the same day of month
                      (task.TaskType == TaskType.Recurring &&
                       task.RecurrencePattern.RecurrenceType == RecurrenceType.Monthly &&
                       task.StartDate.Day == today.Day)
                  )
            select task;

        // Get the list of tasks that match the base criteria
        var baseTasks = await AsyncExecuter.ToListAsync(baseQuery);

        // Get weekly recurring tasks separately
        var weeklyQuery = from task in query
            join userGroup in userTaskGroups on task.TaskGroupId equals userGroup.TaskGroupId
            where userGroup.UserId == userId &&
                  task.StartDate.Date <= today &&
                  (task.EndDate == null || task.EndDate.Value.Date >= today) &&
                  task.TaskType == TaskType.Recurring &&
                  task.RecurrencePattern.RecurrenceType == RecurrenceType.Weekly
            select task;

        // Evaluate the weekly tasks in memory
        var weeklyTasks = await AsyncExecuter.ToListAsync(weeklyQuery);
        var weeklyTasksForToday =
            weeklyTasks.Where(t => t.RecurrencePattern!.DaysOfWeek!.Contains(today.DayOfWeek)).ToList();

        // Combine the results
        var result = baseTasks.Union(weeklyTasksForToday).ToList();
        return result;
    }


    public async Task<List<TaskItem>> GetTasksForNextNDaysAsync(Guid userId, int days)
    {
        var query = await _taskItemRepository.GetQueryableAsync();
        var userTaskGroups = await _userTaskGroupRepository.GetQueryableAsync();

        var today = Clock.Now.Date;
        var endDate = today.AddDays(days);

        // Get one-time tasks that fall within the date range
        var oneTimeTasks = from task in query
            join userGroup in userTaskGroups on task.TaskGroupId equals userGroup.TaskGroupId
            where userGroup.UserId == userId &&
                  task.TaskType == TaskType.OneTime &&
                  task.StartDate.Date >= today &&
                  task.StartDate.Date <= endDate &&
                  (task.EndDate == null || task.EndDate.Value.Date >= today)
            select task;

        // Get recurring tasks that are active during this period
        // (Without filtering by specific recurrence rules yet)
        var recurringTasksQuery = from task in query
            join userGroup in userTaskGroups on task.TaskGroupId equals userGroup.TaskGroupId
            where userGroup.UserId == userId &&
                  task.TaskType == TaskType.Recurring &&
                  task.StartDate.Date <= endDate &&
                  (task.EndDate == null || task.EndDate.Value.Date >= today) &&
                  (task.RecurrencePattern.EndDate == null || task.RecurrencePattern.EndDate.Value.Date >= today)
            select task;

        // Execute the queries
        var oneTimeTasksList = await AsyncExecuter.ToListAsync(oneTimeTasks);
        var recurringTasksList = await AsyncExecuter.ToListAsync(recurringTasksQuery);

        // For recurring tasks, we need to filter them further based on recurrence pattern
        var recurringTasksForPeriod = new List<TaskItem>();

        foreach (var task in recurringTasksList)
        {
            // For daily tasks, they occur every day in the period
            if (task.RecurrencePattern!.RecurrenceType == RecurrenceType.Daily)
            {
                recurringTasksForPeriod.Add(task);
                continue;
            }

            // For weekly tasks, check if they occur on any day in the period
            if (task.RecurrencePattern.RecurrenceType == RecurrenceType.Weekly)
            {
                // Find which days in the date range match the DaysOfWeek pattern
                for (var date = today; date <= endDate; date = date.AddDays(1))
                {
                    if (task.RecurrencePattern.DaysOfWeek!.Contains(date.DayOfWeek))
                    {
                        recurringTasksForPeriod.Add(task);
                        break; // Once we know this task is relevant, we can stop checking
                    }
                }
            }

            // For monthly tasks, check if they occur on any day in the period
            if (task.RecurrencePattern.RecurrenceType == RecurrenceType.Monthly)
            {
                // Check if the day of month falls within our date range
                var dayOfMonth = task.StartDate.Day;

                // Check each day in the range to see if it matches the monthly pattern
                for (var date = today; date <= endDate; date = date.AddDays(1))
                {
                    if (date.Day == dayOfMonth)
                    {
                        recurringTasksForPeriod.Add(task);
                        break; // Once we know this task is relevant, we can stop checking
                    }
                }
            }
        }

        // Combine the one-time tasks and the filtered recurring tasks
        var result = oneTimeTasksList.Union(recurringTasksForPeriod).ToList();
        return result;
    }
}