using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.Dtos;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.TaskGroups;
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

    public TaskGroupAppService(
        IRepository<TaskGroup, Guid> repository,
        ITaskGroupManager taskGroupManager,
        IIdentityUserRepository userRepository,
        IRepository<UserTaskGroup, Guid> userTaskGroupRepository,
        ICurrentUser currentUser)
        : base(repository)
    {
        _taskGroupManager = taskGroupManager;
        _userRepository = userRepository;
        _userTaskGroupRepository = userTaskGroupRepository;
        _currentUser = currentUser;
    }

    public override async Task<TaskGroupDto> GetAsync(Guid id)
    {
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(id);
        return ObjectMapper.Map<TaskGroup, TaskGroupDto>(taskGroup);
    }

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

    public async Task MarkAsCompletedAsync(Guid id)
    {
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(id);
        var currentUserId = _currentUser.GetId();

        foreach (var task in taskGroup.Tasks)
        {
            var userProgress = task.UserProgresses.FirstOrDefault(up => up.UserId == currentUserId);
            if (userProgress != null && !userProgress.IsCompleted)
            {
                await _taskGroupManager.MarkProgressAsCompletedAsync(id, task.Id, currentUserId);
            }
        }
    }

    public async Task MarkAsIncompleteAsync(Guid id)
    {
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(id);
        var currentUserId = _currentUser.GetId();

        foreach (var task in taskGroup.Tasks)
        {
            var userProgress = task.UserProgresses.FirstOrDefault(up => up.UserId == currentUserId);
            if (userProgress != null && userProgress.IsCompleted)
            {
                await _taskGroupManager.MarkProgressAsIncompletedAsync(id, task.Id, currentUserId);
            }
        }
    }

    public async Task<UserTaskGroupDto> AddUserAsync(Guid taskGroupId, Guid userId, UserTaskGroupRole role)
    {
        var userTaskGroup = await _taskGroupManager.AddUserToGroupAsync(taskGroupId, userId, role);

        var dto = ObjectMapper.Map<UserTaskGroup, UserTaskGroupDto>(userTaskGroup);
        var user = await _userRepository.GetAsync(userId);
        dto.UserName = user.UserName;

        return dto;
    }

    public async Task RemoveUserAsync(Guid taskGroupId, Guid userId)
    {
        await _taskGroupManager.RemoveUserFromGroupAsync(taskGroupId, userId);
    }

    public async Task<UserTaskGroupDto> ChangeUserRoleAsync(Guid taskGroupId, Guid userId, UserTaskGroupRole newRole)
    {
        await _taskGroupManager.ChangeUserGroupPermissionAsync(taskGroupId, userId, newRole);

        var userTaskGroup =
            await _userTaskGroupRepository.GetAsync(utg => utg.TaskGroupId == taskGroupId && utg.UserId == userId);

        var dto = ObjectMapper.Map<UserTaskGroup, UserTaskGroupDto>(userTaskGroup);
        var user = await _userRepository.GetAsync(userId);
        dto.UserName = user.UserName;

        return dto;
    }
}