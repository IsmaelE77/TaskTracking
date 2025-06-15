using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TaskTracking.Permissions;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using TaskTracking.Permissions.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
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
    private readonly IDataFilter _dataFilter;
    private readonly IUserTaskGroupRoleCacheService _roleCacheService;
    private readonly CurrentUserTaskGroups _currentUserTaskGroups;

    public TaskGroupAppService(
        IRepository<TaskGroup, Guid> repository,
        ITaskGroupManager taskGroupManager,
        IIdentityUserRepository userRepository,
        IRepository<UserTaskGroup, Guid> userTaskGroupRepository,
        ICurrentUser currentUser,
        IReadOnlyRepository<TaskGroup, Guid> taskGroupRepository,
        IDataFilter dataFilter,
        IUserTaskGroupRoleCacheService roleCacheService,
        CurrentUserTaskGroups currentUserTaskGroups)
        : base(repository)
    {
        _taskGroupManager = taskGroupManager;
        _userRepository = userRepository;
        _userTaskGroupRepository = userTaskGroupRepository;
        _currentUser = currentUser;
        _taskGroupRepository = taskGroupRepository;
        _dataFilter = dataFilter;
        _roleCacheService = roleCacheService;
        _currentUserTaskGroups = currentUserTaskGroups;
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

    public override async Task<TaskGroupDto> CreateAsync(CreateTaskGroupDto input)
    {
        var currentUserId = _currentUser.GetId();

        var taskGroup = await _taskGroupManager.CreateAsync(
            input.Title,
            input.Description,
            input.StartDate,
            input.EndDate,
            currentUserId);

        await _roleCacheService.ClearAsync(currentUserId, taskGroup.Id);

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
        // Get all users in the task group before deletion to clear their caches
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(id);
        var userIds = taskGroup.UserTaskGroups.Select(utg => utg.UserId).ToList();

        await _taskGroupManager.DeleteAsync(id);

        // Clear caches for all affected users
        foreach (var userId in userIds)
        {
            await _roleCacheService.ClearAsync(userId, id);
            await _currentUserTaskGroups.ClearCacheAsync(userId);
        }
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

        // Clear caches for the affected user
        await _roleCacheService.ClearAsync(userId, id);
        await _currentUserTaskGroups.ClearCacheAsync(userId);

        var dto = ObjectMapper.Map<UserTaskGroup, UserTaskGroupDto>(userTaskGroup);
        var user = await _userRepository.GetAsync(userId);
        dto.UserName = user.UserName;

        return dto;
    }

    [Authorize(UserTaskGroupPermissions.ManageUsers)]
    public async Task RemoveUserAsync(Guid id, Guid userId)
    {
        await _taskGroupManager.RemoveUserFromGroupAsync(id, userId);

        // Clear caches for the affected user
        await _roleCacheService.ClearAsync(userId, id);
        await _currentUserTaskGroups.ClearCacheAsync(userId);
    }

    [Authorize(UserTaskGroupPermissions.ManageUsers)]
    public async Task<List<UserTaskGroupDto>> GetTaskGroupUsersAsync(Guid id)
    {
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(id);
        var userTaskGroups = taskGroup.UserTaskGroups.ToList();
        var dtos = new List<UserTaskGroupDto>();

        foreach (var userTaskGroup in userTaskGroups)
        {
            var dto = ObjectMapper.Map<UserTaskGroup, UserTaskGroupDto>(userTaskGroup);
            var user = await _userRepository.GetAsync(userTaskGroup.UserId);
            dto.UserName = user.UserName;
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<PagedResultDto<UserSearchResultDto>> SearchUsersAsync(SearchUsersInput input)
    {
        // Get users already in the task group first
        var userTaskGroupQueryable = await _userTaskGroupRepository.GetQueryableAsync();
        var existingUserIds = await AsyncExecuter.ToListAsync(
            userTaskGroupQueryable
                .Where(utg => utg.TaskGroupId == input.TaskGroupId)
                .Select(utg => utg.UserId));

        // Get all users and filter
        var allUsers = await _userRepository.GetListAsync();
        var filteredUsers = allUsers.AsQueryable();

        // Filter by keyword if provided
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim().ToLower();
            filteredUsers = filteredUsers.Where(u =>
                u.UserName.ToLower().Contains(keyword) ||
                u.Email.ToLower().Contains(keyword) ||
                (u.Name != null && u.Name.ToLower().Contains(keyword)));
        }

        var totalCount = filteredUsers.Count();
        var users = filteredUsers
            .OrderBy(u => u.UserName)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        var dtos = users.Select(user => new UserSearchResultDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Name = user.Name ?? string.Empty,
            IsAlreadyInGroup = existingUserIds.Contains(user.Id)
        }).ToList();

        return new PagedResultDto<UserSearchResultDto>(totalCount, dtos);
    }

    [Authorize(UserTaskGroupPermissions.ManageUsers)]
    public async Task<UserTaskGroupDto> UpdateUserRoleAsync(Guid id, Guid userId, UserTaskGroupRole newRole)
    {
        await _taskGroupManager.ChangeUserGroupPermissionAsync(id, userId, newRole);

        // Clear caches for the affected user (role changed, so role cache needs clearing)
        await _roleCacheService.ClearAsync(userId, id);

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

    [Authorize(UserTaskGroupPermissions.RecordProgress)]
    public async Task RemoveTaskProgressAsync(Guid id, RemoveTaskProgressDto input)
    {
        var userId = _currentUser.GetId();

        await _taskGroupManager.RemoveTaskProgressAsync(
            id,
            input.TaskItemId,
            userId,
            input.Date);
    }

    public async Task<TaskProgressDetailDto> GetTaskProgressDetailAsync(Guid taskGroupId, Guid taskItemId)
    {
        var userId = _currentUser.GetId();
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(taskGroupId);

        var taskItem = taskGroup.Tasks.FirstOrDefault(t => t.Id == taskItemId);
        if (taskItem == null)
        {
            throw new EntityNotFoundException(typeof(TaskItem), taskItemId);
        }

        var userProgress = taskItem.UserProgresses.FirstOrDefault(up => up.UserId == userId);

        var result = new TaskProgressDetailDto
        {
            TaskItem = ObjectMapper.Map<TaskItem, TaskItemDto>(taskItem),
            UserProgress = userProgress != null ? ObjectMapper.Map<UserTaskProgress, UserTaskProgressDto>(userProgress) : null,
            TotalDueCount = taskItem.GetDueCount(),
            CompletedCount = userProgress?.ProgressEntries.Count ?? 0,
            IsFullyCompleted = userProgress?.ProgressPercentage == 100,
            IsDueToday = taskItem.IsDue(DateTime.Today, CurrentUser.GetId()),
            CanRecordToday = userProgress != null && !userProgress.ProgressEntries.Any(pe => pe.Date == DateOnly.FromDateTime(DateTime.Today))
        };

        // Calculate due dates for the task
        result.DueDates = CalculateDueDates(taskItem);

        // Get completed dates
        if (userProgress != null)
        {
            result.CompletedDates = userProgress.ProgressEntries.Select(pe => pe.Date).ToList();
        }

        return result;
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

    private List<DateOnly> CalculateDueDates(TaskItem taskItem)
    {
        var dueDates = new List<DateOnly>();

        if (taskItem.TaskType == TaskType.OneTime)
        {
            dueDates.Add(DateOnly.FromDateTime(taskItem.StartDate));
            return dueDates;
        }

        if (taskItem.RecurrencePattern == null)
            return dueDates;

        var startDate = taskItem.StartDate;
        var endDate = taskItem.RecurrencePattern.EndDate ?? taskItem.EndDate ?? DateTime.Today.AddYears(1);
        var current = startDate;

        switch (taskItem.RecurrencePattern.RecurrenceType)
        {
            case RecurrenceType.Daily:
                while (current <= endDate)
                {
                    dueDates.Add(DateOnly.FromDateTime(current));

                    if (taskItem.RecurrencePattern.Occurrences.HasValue &&
                        dueDates.Count >= taskItem.RecurrencePattern.Occurrences.Value)
                        break;

                    current = current.AddDays(taskItem.RecurrencePattern.Interval);
                }
                break;

            case RecurrenceType.Weekly:
                var weeks = 0;
                while (current <= endDate)
                {
                    foreach (var day in taskItem.RecurrencePattern.DaysOfWeek)
                    {
                        var candidate = startDate.AddDays((weeks * 7 * taskItem.RecurrencePattern.Interval) +
                                                         (int)day - (int)startDate.DayOfWeek);

                        if (candidate < startDate || candidate > endDate)
                            continue;

                        dueDates.Add(DateOnly.FromDateTime(candidate));

                        if (taskItem.RecurrencePattern.Occurrences.HasValue &&
                            dueDates.Count >= taskItem.RecurrencePattern.Occurrences.Value)
                            return dueDates;
                    }
                    weeks++;
                    current = startDate.AddDays(weeks * 7 * taskItem.RecurrencePattern.Interval);
                }
                break;

            case RecurrenceType.Monthly:
                while (current <= endDate)
                {
                    dueDates.Add(DateOnly.FromDateTime(current));

                    if (taskItem.RecurrencePattern.Occurrences.HasValue &&
                        dueDates.Count >= taskItem.RecurrencePattern.Occurrences.Value)
                        break;

                    current = current.AddMonths(taskItem.RecurrencePattern.Interval);
                }
                break;
        }

        return dueDates.OrderBy(d => d).ToList();
    }
    
    [Authorize(UserTaskGroupPermissions.GenerateInvitations)]
    public async Task<TaskGroupInvitationDto> GenerateInvitationLinkAsync(
        Guid id,
        CreateTaskGroupInvitationDto input)
    {
        var currentUserId = _currentUser.GetId();

        var invitation = await _taskGroupManager.CreateInvitationAsync(
            id,
            currentUserId,
            input.ExpirationHours,
            input.MaxUses,
            input.DefaultRole);

        var dto = ObjectMapper.Map<TaskGroupInvitation, TaskGroupInvitationDto>(invitation);

        // Set additional properties
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(id);
        dto.TaskGroupTitle = taskGroup.Title;

        var createdByUser = await _userRepository.GetAsync(invitation.CreatedByUserId);
        dto.CreatedByUserName = createdByUser.UserName;

        dto.IsValid = invitation.IsValid();
        dto.IsExpired = invitation.IsExpired();
        dto.IsMaxUsesReached = invitation.IsMaxUsesReached();

        // Store invitation token for URL generation in client
        dto.InvitationToken = invitation.InvitationToken;

        return dto;
    }

    [AllowAnonymous]
    public async Task<TaskGroupInvitationDetailsDto> GetInvitationDetailsAsync(string invitationToken)
    {
        using var _ = _dataFilter.Disable<IHaveTaskGroup>();
        using var __ = _dataFilter.Disable<IAccessibleTaskGroup>();

        var invitation = await _taskGroupManager.GetInvitationByTokenAsync(invitationToken);
        var taskGroup = await _taskGroupManager.GetWithDetailsAsync(invitation.TaskGroupId);
        var createdByUser = await _userRepository.GetAsync(invitation.CreatedByUserId);

        var dto = new TaskGroupInvitationDetailsDto
        {
            TaskGroupId = invitation.TaskGroupId,
            TaskGroupTitle = taskGroup.Title,
            TaskGroupDescription = taskGroup.Description,
            InvitationToken = invitation.InvitationToken,
            ExpirationDate = invitation.ExpirationDate,
            CreatedByUserName = createdByUser.UserName,
            DefaultRole = invitation.DefaultRole,
            IsValid = invitation.IsValid(),
            IsExpired = invitation.IsExpired(),
            IsMaxUsesReached = invitation.IsMaxUsesReached(),
            MaxUses = invitation.MaxUses,
            CurrentUses = invitation.CurrentUses,
            CreationTime = invitation.CreationTime
        };

        return dto;
    }

    [Authorize]
    public async Task<UserTaskGroupDto> JoinTaskGroupByInvitationAsync(JoinTaskGroupByInvitationDto input)
    {
        using var _ = _dataFilter.Disable<IHaveTaskGroup>();
        using var __ = _dataFilter.Disable<IAccessibleTaskGroup>();
        
        var currentUserId = _currentUser.GetId();

        var userTaskGroup = await _taskGroupManager.JoinTaskGroupByInvitationAsync(
            input.InvitationToken,
            currentUserId);

        // Clear caches for the current user who just joined
        await _roleCacheService.ClearAsync(currentUserId, userTaskGroup.TaskGroupId);
        await _currentUserTaskGroups.ClearCacheAsync(currentUserId);

        var dto = ObjectMapper.Map<UserTaskGroup, UserTaskGroupDto>(userTaskGroup);
        var user = await _userRepository.GetAsync(currentUserId);
        dto.UserName = user.UserName;

        return dto;
    }

    [Authorize(UserTaskGroupPermissions.GenerateInvitations)]
    public async Task<List<TaskGroupInvitationDto>> GetTaskGroupInvitationsAsync(Guid id)
    {
        var invitations = await _taskGroupManager.GetTaskGroupInvitationsAsync(id);
        var dtos = new List<TaskGroupInvitationDto>();

        foreach (var invitation in invitations)
        {
            var dto = ObjectMapper.Map<TaskGroupInvitation, TaskGroupInvitationDto>(invitation);

            var taskGroup = await _taskGroupManager.GetWithDetailsAsync(invitation.TaskGroupId);
            dto.TaskGroupTitle = taskGroup.Title;

            var createdByUser = await _userRepository.GetAsync(invitation.CreatedByUserId);
            dto.CreatedByUserName = createdByUser.UserName;

            dto.IsValid = invitation.IsValid();
            dto.IsExpired = invitation.IsExpired();
            dto.IsMaxUsesReached = invitation.IsMaxUsesReached();
            dto.InvitationToken = invitation.InvitationToken;

            dtos.Add(dto);
        }

        return dtos;
    }

    [Authorize(UserTaskGroupPermissions.GenerateInvitations)]
    public async Task DeleteInvitationAsync(Guid invitationId)
    {
        var currentUserId = _currentUser.GetId();
        await _taskGroupManager.DeleteInvitationAsync(invitationId, currentUserId);
    }

    public async Task<UserTaskGroupRole> GetUserRoleAsync(Guid id)
    {
        var group = await _taskGroupRepository.GetAsync(id);

        var role = group!.UserTaskGroups.FirstOrDefault(utg => utg.UserId == _currentUser.Id)?.Role;

        if (role is null)
        {
            throw new EntityNotFoundException();
        }

        return role.Value;
    }
}