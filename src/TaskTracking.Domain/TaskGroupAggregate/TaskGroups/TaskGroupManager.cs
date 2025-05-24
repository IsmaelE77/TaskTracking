using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace TaskTracking.TaskGroupAggregate.TaskGroups;

public class TaskGroupManager : DomainService, ITaskGroupManager
{
    private readonly IRepository<TaskGroup, Guid> _taskGroupRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IReadOnlyRepository<UserTaskGroup, Guid> _userTaskGroupReadOnlyRepository;

    public TaskGroupManager(
        IRepository<TaskGroup, Guid> taskGroupRepository,
        IIdentityUserRepository userRepository,
        IReadOnlyRepository<UserTaskGroup, Guid> userTaskGroupReadOnlyRepository)
    {
        _taskGroupRepository = taskGroupRepository;
        _userRepository = userRepository;
        _userTaskGroupReadOnlyRepository = userTaskGroupReadOnlyRepository;
    }

    public async Task<TaskGroup> CreateAsync(
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate,
        Guid creatorUserId)
    {
        var taskGroup = new TaskGroup(
            GuidGenerator.Create(),
            title,
            description,
            startDate,
            endDate);

        // Add the creator as the owner of the group
        var userTaskGroup = new UserTaskGroup(
            GuidGenerator.Create(),
            creatorUserId,
            taskGroup.Id,
            UserTaskGroupRole.Owner);

        taskGroup.AddUserTaskGroup(userTaskGroup);


        return await _taskGroupRepository.InsertAsync(taskGroup);
    }

    public async Task<TaskGroup> UpdateAsync(
        Guid id,
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate)
    {
        var taskGroup = await _taskGroupRepository.GetAsync(id);
        taskGroup.SetTitle(title);
        taskGroup.SetDescription(description);
        taskGroup.SetValidityPeriod(startDate, endDate);
        return await _taskGroupRepository.UpdateAsync(taskGroup);
    }

    public async Task<Guid> DeleteAsync(Guid id)
    {
        var taskGroup = await _taskGroupRepository.GetAsync(id);
        await _taskGroupRepository.DeleteAsync(taskGroup);
        return taskGroup.Id;
    }

    public async Task<TaskItem> CreateTaskItemAsync(
        Guid taskGroupId,
        string title,
        string description,
        DateTime startDate,
        DateTime? endDate,
        TaskType taskType,
        RecurrencePattern recurrencePattern = null)
    {
        var taskGroup = await GetWithDetailsAsync(taskGroupId);

        TaskItem taskItem;

        if (taskType == TaskType.OneTime)
        {
            taskItem = TaskItem.CreateOneTimeTask(
                GuidGenerator.Create(),
                title,
                description,
                startDate,
                endDate,
                taskGroupId);
        }
        else
        {
            if (taskType == TaskType.Recurring && recurrencePattern == null)
            {
                throw new BusinessException(TaskTrackingDomainErrorCodes.RecurrencePatternRequired);
            }

            taskItem = TaskItem.CreateRecurringTask(
                GuidGenerator.Create(),
                title,
                description,
                startDate,
                endDate,
                recurrencePattern,
                taskGroupId);
        }

        foreach (var userTaskGroup in taskGroup.UserTaskGroups)
        {
            var progress = new UserTaskProgress(
                GuidGenerator.Create(),
                userTaskGroup.UserId,
                taskItem.Id);

            taskItem.AddUserProgress(progress);
        }


        taskGroup.AddTask(taskItem);

        await _taskGroupRepository.UpdateAsync(taskGroup);

        return taskItem;
    }

    public async Task<TaskItem> UpdateTaskItemAsync(
        Guid taskGroupId,
        Guid taskItemId,
        string title,
        string description,
        TaskType taskType,
        DateTime startDate,
        DateTime? endDate,
        RecurrencePattern? recurrencePattern = null)
    {
        var taskGroup = await GetWithDetailsAsync(taskGroupId);

        var taskItem = taskGroup.UpdateTaskItem(taskItemId, title, description, startDate, taskType, endDate, recurrencePattern);

        await _taskGroupRepository.UpdateAsync(taskGroup);

        return taskItem;
    }

    public async Task<TaskItem> DeleteTaskItemAsync(
        Guid taskGroupId,
        Guid taskItemId)
    {
        var taskGroup = await GetWithDetailsAsync(taskGroupId);

        var taskItem = taskGroup.DeleteTaskItem(taskItemId);

        await _taskGroupRepository.UpdateAsync(taskGroup);

        return taskItem;
    }


    public async Task<UserTaskGroup> AddUserToGroupAsync(
        Guid taskGroupId,
        Guid userId,
        UserTaskGroupRole role)
    {
        var taskGroup = await GetWithDetailsAsync(taskGroupId);
        var user = await _userRepository.GetAsync(userId);


        if (taskGroup.UserTaskGroups.Any(x => x.UserId == userId && x.TaskGroupId == taskGroupId))
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserAlreadyInGroup);
        }

        var userTaskGroup = new UserTaskGroup(
            GuidGenerator.Create(),
            userId,
            taskGroupId,
            role);

        foreach (var task in taskGroup.Tasks)
        {
            var progress = new UserTaskProgress(
                GuidGenerator.Create(),
                userTaskGroup.UserId,
                task.Id);

            task.AddUserProgress(progress);
        }

        taskGroup.AddUserTaskGroup(userTaskGroup);

        await _taskGroupRepository.UpdateAsync(taskGroup);

        return userTaskGroup;
    }

    public async Task RemoveUserFromGroupAsync(Guid taskGroupId, Guid userId)
    {
        var taskGroup = await GetWithDetailsAsync(taskGroupId);
        var user = await _userRepository.GetAsync(userId);

        taskGroup.RemoveUserTaskGroup(userId);

        await _taskGroupRepository.UpdateAsync(taskGroup);
    }

    public async Task ChangeUserGroupPermissionAsync(Guid taskGroupId, Guid userId, UserTaskGroupRole newRole)
    {
        var taskGroup = await GetWithDetailsAsync(taskGroupId);

        taskGroup.ChangeUserPermission(userId, newRole);
        await _taskGroupRepository.UpdateAsync(taskGroup);
    }

    public async Task RecordTaskProgressAsync(
        Guid taskGroupId,
        Guid taskItemId,
        Guid userId,
        DateOnly date)
    {
        var taskGroup = await GetWithDetailsAsync(taskGroupId);
        taskGroup.RecordTaskProgress(taskItemId, userId, date);
        await _taskGroupRepository.UpdateAsync(taskGroup);
    }

    public async Task<TaskGroup> GetWithDetailsAsync(Guid taskGroupId)
    {
        var query = await _taskGroupRepository.WithDetailsAsync();

        var taskGroup = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == taskGroupId));

        if (taskGroup is null)
        {
            throw new EntityNotFoundException(typeof(TaskGroup), taskGroupId);
        }

        return taskGroup;
    }

    /// <summary>
    /// Gets all task groups for a specific user.
    /// </summary>
    public async Task<(List<TaskGroup> Items, int TotalCount)> GetUserTaskGroupsAsync(Guid userId, int skipCount, int maxResultCount)
    {
        var userTaskGroupsQueryable = await _userTaskGroupReadOnlyRepository.GetQueryableAsync();
        userTaskGroupsQueryable = userTaskGroupsQueryable
            .Where(utg => utg.UserId == userId);

        var totalCount = await AsyncExecuter.CountAsync(userTaskGroupsQueryable);
        var taskGroupIds = await AsyncExecuter.ToListAsync(
            userTaskGroupsQueryable
                .Select(utg => utg.Id)
                .PageBy(skipCount, maxResultCount)
        );

        var result = await _taskGroupRepository.GetListAsync(tg => taskGroupIds.Contains(tg.Id));

        return (result, totalCount);
    }

    /// <summary>
    /// Gets all active task groups for a specific user.
    /// </summary>
    public async Task<(List<TaskGroup> Items, int TotalCount)> GetUserActiveTaskGroupsAsync(Guid userId, int skipCount, int maxResultCount)
    {
        var userTaskGroupsQueryable = await _userTaskGroupReadOnlyRepository.GetQueryableAsync();
        userTaskGroupsQueryable = userTaskGroupsQueryable
            .Where(utg => utg.UserId == userId);

        var totalCount = await AsyncExecuter.CountAsync(userTaskGroupsQueryable);
        var taskGroupIds = await AsyncExecuter.ToListAsync(
            userTaskGroupsQueryable
                .Select(utg => utg.TaskGroupId)
                .PageBy(skipCount, maxResultCount)
        );

        var today = Clock.Now.Date;

        var result = await _taskGroupRepository.GetListAsync(tg =>
            taskGroupIds.Contains(tg.Id) &&
            tg.StartDate <= today &&
            (!tg.EndDate.HasValue || tg.EndDate.Value >= today));

        return (result, totalCount);
    }

    /// <summary>
    /// Gets all task groups owned by a specific user.
    /// </summary>
    public async Task<(List<TaskGroup> Items, int TotalCount)> GetUserOwnedTaskGroupsAsync(Guid userId, int skipCount, int maxResultCount)
    {
        var userTaskGroupsQueryable = await _userTaskGroupReadOnlyRepository.GetQueryableAsync();
        userTaskGroupsQueryable = userTaskGroupsQueryable
            .Where(utg => utg.UserId == userId && utg.Role == UserTaskGroupRole.Owner);

        var totalCount = await AsyncExecuter.CountAsync(userTaskGroupsQueryable);

        var taskGroupIds =await AsyncExecuter.ToListAsync(
            userTaskGroupsQueryable
                .Select(utg => utg.TaskGroupId)
                .PageBy(skipCount, maxResultCount)
        );

        var items = await _taskGroupRepository.GetListAsync(tg => taskGroupIds.Contains(tg.Id));

        return (items, totalCount);
    }
}