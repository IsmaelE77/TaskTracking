using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TaskTracking.Permissions;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace TaskTracking.TaskGroupAggregate;

public class TaskGroupAppService :
    CrudAppService<
        TaskGroup,
        TaskGroupDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateTaskGroupDto,
        UpdateTaskGroupDto>,
    ITaskGroupAppService
{
    private readonly ITaskGroupManager _taskGroupManager;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IRepository<UserTaskGroup, Guid> _userTaskGroupRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IReadOnlyRepository<TaskGroup, Guid> _taskGroupRepository;

    public TaskGroupAppService(
        IRepository<TaskGroup, Guid> repository,
        ITaskGroupManager taskGroupManager,
        IIdentityUserRepository userRepository,
        IRepository<UserTaskGroup, Guid> userTaskGroupRepository,
        ICurrentUser currentUser, IReadOnlyRepository<TaskGroup, Guid> taskGroupRepository)
        : base(repository)
    {
        _taskGroupManager = taskGroupManager;
        _userRepository = userRepository;
        _userTaskGroupRepository = userTaskGroupRepository;
        _currentUser = currentUser;
        _taskGroupRepository = taskGroupRepository;
    }

    public override async Task<PagedResultDto<TaskGroupDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var taskGroupQuery = await _taskGroupRepository.WithDetailsAsync();
        var taskGroupsCount = await AsyncExecuter.CountAsync(taskGroupQuery);
        var taskGroups = await AsyncExecuter.ToListAsync(taskGroupQuery.PageBy(input));
        var taskGroupsDtos = ObjectMapper.Map<List<TaskGroup>,List<TaskGroupDto>>(taskGroups);

        return new PagedResultDto<TaskGroupDto>(taskGroupsCount, taskGroupsDtos);
    }

    public override async Task<TaskGroupDto> GetAsync(Guid id)
    {
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(id);
        return ObjectMapper.Map<TaskGroup, TaskGroupDto>(taskGroup);
    }

    [Authorize(UserTaskGroupPermissions.Create)]
    public override async Task<TaskGroupDto> CreateAsync(CreateTaskGroupDto input)
    {
        var currentUserId = _currentUser.GetId();

        var taskGroup = await _taskGroupManager.CreateAsync(
            input.Title,
            input.Description,
            input.StartDate,
            input.EndDate,
            currentUserId);

        return ObjectMapper.Map<TaskGroup, TaskGroupDto>(taskGroup);
    }

    [Authorize(UserTaskGroupPermissions.Update)]
    public override async Task<TaskGroupDto> UpdateAsync(Guid id, UpdateTaskGroupDto input)
    {
        var taskGroup = await _taskGroupManager.UpdateAsync(
            id,
            input.Title,
            input.Description,
            input.StartDate,
            input.EndDate);

        return ObjectMapper.Map<TaskGroup, TaskGroupDto>(taskGroup);
    }

    [Authorize(UserTaskGroupPermissions.Delete)]
    public override async Task DeleteAsync(Guid id)
    {
        await _taskGroupManager.DeleteAsync(id);
    }

    public async Task<PagedResultDto<TaskGroupDto>> GetMyOwnedTaskGroupsAsync(PagedResultRequestDto input)
    {
        var currentUserId = _currentUser.GetId();
        var taskGroups =
            await _taskGroupManager.GetUserOwnedTaskGroupsAsync(currentUserId, input.SkipCount, input.MaxResultCount);

        var taskGroupsDtos = ObjectMapper.Map<List<TaskGroup>, List<TaskGroupDto>>(taskGroups.Items);

        return new PagedResultDto<TaskGroupDto>(taskGroups.TotalCount, taskGroupsDtos);
    }

    public async Task<PagedResultDto<TaskGroupDto>> GetMyActiveTaskGroupsAsync(PagedResultRequestDto input)
    {
        var currentUserId = _currentUser.GetId();
        var taskGroups = await _taskGroupManager.GetUserActiveTaskGroupsAsync(currentUserId, input.SkipCount, input.MaxResultCount);

        var taskGroupsDtos = ObjectMapper.Map<List<TaskGroup>, List<TaskGroupDto>>(taskGroups.Items);

        return new PagedResultDto<TaskGroupDto>(taskGroups.TotalCount, taskGroupsDtos);
    }

    [Authorize(UserTaskGroupPermissions.ManageUsers)]
    public async Task<UserTaskGroupDto> AddUserAsync(Guid id, Guid userId, UserTaskGroupRole role)
    {
        var userTaskGroup = await _taskGroupManager.AddUserToGroupAsync(id, userId, role);

        var dto = ObjectMapper.Map<UserTaskGroup, UserTaskGroupDto>(userTaskGroup);
        var user = await _userRepository.GetAsync(userId);
        dto.UserName = user.UserName;

        return dto;
    }

    [Authorize(UserTaskGroupPermissions.ManageUsers)]
    public async Task RemoveUserAsync(Guid id, Guid userId)
    {
        await _taskGroupManager.RemoveUserFromGroupAsync(id, userId);
    }

    [Authorize(UserTaskGroupPermissions.ManageUsers)]
    public async Task<UserTaskGroupDto> UpdateUserRoleAsync(Guid id, Guid userId, UserTaskGroupRole newRole)
    {
        await _taskGroupManager.ChangeUserGroupPermissionAsync(id, userId, newRole);

        var userTaskGroup =
            await _userTaskGroupRepository.GetAsync(utg => utg.TaskGroupId == id && utg.UserId == userId);

        var dto = ObjectMapper.Map<UserTaskGroup, UserTaskGroupDto>(userTaskGroup);
        var user = await _userRepository.GetAsync(userId);
        dto.UserName = user.UserName;

        return dto;
    }

    [Authorize(UserTaskGroupPermissions.RecordProgress)]
    public async Task RecordTaskProgressAsync(Guid id, RecordTaskProgressDto input)
    {
        var userId = _currentUser.GetId();

        await _taskGroupManager.RecordTaskProgressAsync(
            id,
            input.TaskItemId,
            userId,
            input.Date);
    }

    [Authorize(UserTaskGroupPermissions.CreateTaskItems)]
    public async Task<TaskItemDto> CreateTaskItemAsync(
        Guid id,
        CreateTaskItemDto input)
    {
        RecurrencePattern recurrencePattern = null;

        if (input.TaskType == TaskType.Recurring && input.RecurrencePattern != null)
        {
            recurrencePattern =
                ObjectMapper.Map<CreateRecurrencePatternDto, RecurrencePattern>(input.RecurrencePattern);
        }

        var taskItem = await _taskGroupManager.CreateTaskItemAsync(
            id,
            input.Title,
            input.Description,
            input.StartDate,
            input.EndDate,
            input.TaskType,
            recurrencePattern);

        return ObjectMapper.Map<TaskItem, TaskItemDto>(taskItem);
    }

    [Authorize(UserTaskGroupPermissions.UpdateTaskItems)]
    public async Task<TaskItemDto> UpdateTaskItemAsync(
        Guid id,
        Guid itemTaskId,
        UpdateTaskItemDto input)
    {
        RecurrencePattern recurrencePattern = null;
        if (input.RecurrencePattern != null)
        {
            recurrencePattern =
                ObjectMapper.Map<CreateRecurrencePatternDto, RecurrencePattern>(input.RecurrencePattern);
        }

        var updatedTaskItem = await _taskGroupManager.UpdateTaskItemAsync(
            id,
            itemTaskId,
            input.Title,
            input.Description,
            input.TaskType,
            input.StartDate,
            input.EndDate,
            recurrencePattern);

        return ObjectMapper.Map<TaskItem, TaskItemDto>(updatedTaskItem);
    }

    [Authorize(UserTaskGroupPermissions.DeleteTaskItems)]
    public async Task<TaskItemDto> DeleteTaskItemAsync(
        Guid id,
        Guid taskItemId)
    {
        var taskItem = await _taskGroupManager.DeleteTaskItemAsync(
            id,
            taskItemId);

        return ObjectMapper.Map<TaskItem, TaskItemDto>(taskItem);
    }
}