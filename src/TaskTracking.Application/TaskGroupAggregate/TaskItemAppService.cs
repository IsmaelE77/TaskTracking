using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskItems;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace TaskTracking.TaskGroupAggregate;

public class TaskItemAppService :
    CrudAppService<
        TaskItem,
        TaskItemDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateTaskItemDto,
        UpdateTaskItemDto>,
    ITaskItemAppService
{
    private readonly ITaskGroupManager _taskGroupManager;
    private readonly TaskItemManager _taskItemManager;
    private readonly ICurrentUser _currentUser;

    public TaskItemAppService(
        IRepository<TaskItem, Guid> repository,
        ITaskGroupManager taskGroupManager,
        TaskItemManager taskItemManager,
        ICurrentUser currentUser)
        : base(repository)
    {
        _taskGroupManager = taskGroupManager;
        _taskItemManager = taskItemManager;
        _currentUser = currentUser;
    }

    public override async Task<TaskItemDto> CreateAsync(CreateTaskItemDto input)
    {
        RecurrencePattern recurrencePattern = null;

        if (input.TaskType == TaskType.Recurring && input.RecurrencePattern != null)
        {
            recurrencePattern =
                ObjectMapper.Map<CreateRecurrencePatternDto, RecurrencePattern>(input.RecurrencePattern);
        }

        var taskItem = await _taskGroupManager.CreateTaskItemAsync(
            input.TaskGroupId,
            input.Title,
            input.Description,
            input.StartDate,
            input.EndDate,
            input.TaskType,
            recurrencePattern);

        return ObjectMapper.Map<TaskItem, TaskItemDto>(taskItem);
    }

    public override async Task<TaskItemDto> UpdateAsync(Guid id, UpdateTaskItemDto input)
    {
        var taskItem = await Repository.GetAsync(id);

        RecurrencePattern recurrencePattern = null;
        if (taskItem.TaskType == TaskType.Recurring && input.RecurrencePattern != null)
        {
            recurrencePattern =
                ObjectMapper.Map<CreateRecurrencePatternDto, RecurrencePattern>(input.RecurrencePattern);
        }

        var updatedTaskItem = await _taskGroupManager.UpdateTaskItemAsync(
            taskItem.TaskGroupId,
            id,
            input.Title,
            input.Description,
            input.StartDate,
            input.EndDate,
            recurrencePattern);

        return ObjectMapper.Map<TaskItem, TaskItemDto>(updatedTaskItem);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var taskItem = await Repository.GetAsync(id);
        await _taskGroupManager.DeleteTaskItemAsync(taskItem.TaskGroupId, id);
    }

    public async Task<PagedResultDto<TaskItemDto>> GetTasksForGroupAsync(Guid taskGroupId, PagedResultRequestDto input)
    {
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(taskGroupId);
        var totalCount = taskGroup.Tasks.Count;

        var tasks = taskGroup.Tasks
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        var taskDtos = ObjectMapper.Map<List<TaskItem>, List<TaskItemDto>>(tasks);

        return new PagedResultDto<TaskItemDto>(totalCount, taskDtos);
    }

    public async Task<PagedResultDto<TaskItemDto>> GetMyTasksDueTodayAsync(PagedResultRequestDto input)
    {
        var currentUserId = _currentUser.GetId();

        var tasks = await _taskItemManager.GetTasksForTodayPagedAsync(currentUserId, input.SkipCount,
            input.MaxResultCount);

        var taskDtos = ObjectMapper.Map<List<TaskItem>, List<TaskItemDto>>(tasks.Items);

        return new PagedResultDto<TaskItemDto>(tasks.TotalCount, taskDtos);
    }

    public async Task<PagedResultDto<TaskItemDto>> GetMyTasksDueInNextNDaysAsync(int days, PagedResultRequestDto input)
    {
        var currentUserId = _currentUser.GetId();
        var tasks = await _taskItemManager.GetTasksForNextNDaysPagedAsync(currentUserId, days, input.SkipCount,
            input.MaxResultCount);

        var taskDtos = ObjectMapper.Map<List<TaskItem>, List<TaskItemDto>>(tasks.Items);

        return new PagedResultDto<TaskItemDto>(tasks.TotalCount, taskDtos);
    }

    public async Task RecordTaskProgressAsync(RecordTaskProgressDto input)
    {
        var userId = _currentUser.GetId();

        await _taskGroupManager.RecordTaskProgressAsync(
            input.TaskGroupId,
            input.TaskItemId,
            userId,
            input.Date);
    }
}