using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskItems;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace TaskTracking.TaskGroupAggregate;

public class TaskItemAppService : ApplicationService,ITaskItemAppService
{
    private IReadOnlyRepository<TaskItem, Guid> _taskItemReadOnlyRepository;
    private readonly ITaskGroupManager _taskGroupManager;
    private readonly TaskItemManager _taskItemManager;
    private readonly ICurrentUser _currentUser;

    public TaskItemAppService(
        IReadOnlyRepository<TaskItem, Guid> repository,
        ITaskGroupManager taskGroupManager,
        TaskItemManager taskItemManager,
        ICurrentUser currentUser)
    {
        _taskItemReadOnlyRepository = repository;
        _taskGroupManager = taskGroupManager;
        _taskItemManager = taskItemManager;
        _currentUser = currentUser;
    }

    public async Task<TaskItemDto> GetAsync(Guid id)
    {
        var queryable = await _taskItemReadOnlyRepository.WithDetailsAsync(x => x.UserProgresses);
        
        var taskItem = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));

        if (taskItem is null)
        {
            throw new EntityNotFoundException(typeof(TaskItem), id);
        }
        
        return ObjectMapper.Map<TaskItem, TaskItemDto>(taskItem);
    }

    public async Task<PagedResultDto<TaskItemDto>> GetListAsync(PagedResultRequestDto input)
    {
        var query = await _taskItemReadOnlyRepository.WithDetailsAsync(x => x.UserProgresses);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var taskItems = await AsyncExecuter.ToListAsync(query.PageBy(input.SkipCount, input.MaxResultCount));

        var taskDtos = ObjectMapper.Map<List<TaskItem>, List<TaskItemDto>>(taskItems);

        return new PagedResultDto<TaskItemDto>(totalCount, taskDtos);
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
}