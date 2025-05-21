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

        var today = DateTime.Today;

        query = from task in query
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
                      // Recurring weekly tasks for today's day of week
                      (task.TaskType == TaskType.Recurring &&
                       task.RecurrencePattern.RecurrenceType == RecurrenceType.Weekly &&
                       task.RecurrencePattern.DaysOfWeek.Contains(today.DayOfWeek)) ||
                      // Recurring monthly tasks on the same day of month
                      (task.TaskType == TaskType.Recurring &&
                       task.RecurrencePattern.RecurrenceType == RecurrenceType.Monthly &&
                       task.StartDate.Day == today.Day)
                  )
            select task;

        return await AsyncExecuter.ToListAsync(query);
    }

    public async Task<List<TaskItem>> GetTasksForWeekAsync(Guid userId)
    {
        var query = await _taskItemRepository.GetQueryableAsync();
        var userTaskGroups = await _userTaskGroupRepository.GetQueryableAsync();

        var today = DateTime.Today;
        var weekEnd = today.AddDays(7);

        query = from task in query
            join userGroup in userTaskGroups on task.TaskGroupId equals userGroup.TaskGroupId
            where userGroup.UserId == userId &&
                  task.StartDate.Date <= weekEnd &&
                  (task.EndDate == null || task.EndDate.Value.Date >= today) &&
                  (
                      // One-time tasks within the week
                      (task.TaskType == TaskType.OneTime &&
                       task.StartDate.Date >= today &&
                       task.StartDate.Date <= weekEnd) ||
                      // All recurring tasks that are active in this period
                      (task.TaskType == TaskType.Recurring &&
                       (task.RecurrencePattern.EndDate == null ||
                        task.RecurrencePattern.EndDate.Value.Date >= today))
                  )
            select task;

        return await AsyncExecuter.ToListAsync(query);
    }

    public async Task<List<TaskItem>> GetTasksForMonthAsync(Guid userId)
    {
        var query = await _taskItemRepository.GetQueryableAsync();
        var userTaskGroups = await _userTaskGroupRepository.GetQueryableAsync();

        var today = DateTime.Today;
        var monthEnd = today.AddMonths(1);

        query = from task in query
            join userGroup in userTaskGroups on task.TaskGroupId equals userGroup.TaskGroupId
            where userGroup.UserId == userId &&
                  task.StartDate.Date <= monthEnd &&
                  (task.EndDate == null || task.EndDate.Value.Date >= today) &&
                  (
                      // One-time tasks within the month
                      (task.TaskType == TaskType.OneTime &&
                       task.StartDate.Date >= today &&
                       task.StartDate.Date <= monthEnd) ||
                      // All recurring tasks that are active in this period
                      (task.TaskType == TaskType.Recurring &&
                       (task.RecurrencePattern.EndDate == null ||
                        task.RecurrencePattern.EndDate.Value.Date >= today))
                  )
            select task;

        return await AsyncExecuter.ToListAsync(query);
    }

    public async Task<List<TaskItem>> GetUpcomingTasksAsync(Guid userId, int days)
    {
        var query = await _taskItemRepository.GetQueryableAsync();
        var userTaskGroups = await _userTaskGroupRepository.GetQueryableAsync();

        var today = DateTime.Today;
        var endDate = today.AddDays(days);

        query = from task in query
            join userGroup in userTaskGroups on task.TaskGroupId equals userGroup.TaskGroupId
            where userGroup.UserId == userId &&
                  task.StartDate.Date <= endDate &&
                  (task.EndDate == null || task.EndDate.Value.Date >= today) &&
                  (
                      // One-time tasks within the specified period
                      (task.TaskType == TaskType.OneTime &&
                       task.StartDate.Date >= today &&
                       task.StartDate.Date <= endDate) ||
                      // All recurring tasks that are active in this period
                      (task.TaskType == TaskType.Recurring &&
                       (task.RecurrencePattern.EndDate == null ||
                        task.RecurrencePattern.EndDate.Value.Date >= today))
                  )
            select task;

        return await AsyncExecuter.ToListAsync(query);
    }
}