using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskGroupInvitations;
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
    private readonly IRepository<TaskGroupInvitation, Guid> _invitationRepository;
    private readonly ITaskGroupInvitationTokenGenerator _tokenGenerator;

    public TaskGroupManager(
        IRepository<TaskGroup, Guid> taskGroupRepository,
        IIdentityUserRepository userRepository,
        IReadOnlyRepository<UserTaskGroup, Guid> userTaskGroupReadOnlyRepository,
        IRepository<TaskGroupInvitation, Guid> invitationRepository,
        ITaskGroupInvitationTokenGenerator tokenGenerator)
    {
        _taskGroupRepository = taskGroupRepository;
        _userRepository = userRepository;
        _userTaskGroupReadOnlyRepository = userTaskGroupReadOnlyRepository;
        _invitationRepository = invitationRepository;
        _tokenGenerator = tokenGenerator;
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

        var taskItem = taskGroup.UpdateTaskItem(taskItemId, title, description, startDate, taskType, endDate,
            recurrencePattern);

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

    public async Task RemoveTaskProgressAsync(
        Guid taskGroupId,
        Guid taskItemId,
        Guid userId,
        DateOnly date)
    {
        var taskGroup = await GetWithDetailsAsync(taskGroupId);
        taskGroup.RemoveTaskProgress(taskItemId, userId, date);
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
    public async Task<(List<TaskGroup> Items, int TotalCount)> GetUserTaskGroupsAsync(Guid userId, int skipCount,
        int maxResultCount)
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

        var taskGroupQuery = await _taskGroupRepository.WithDetailsAsync();

        var result = await AsyncExecuter.ToListAsync(
            taskGroupQuery.Where(tg =>
                taskGroupIds.Contains(tg.Id)));

        return (result, totalCount);
    }

    /// <summary>
    /// Gets all active task groups for a specific user.
    /// </summary>
    public async Task<(List<TaskGroup> Items, int TotalCount)> GetUserActiveTaskGroupsAsync(Guid userId, int skipCount,
        int maxResultCount)
    {
        var today = Clock.Now.Date;

        var taskGroupQuery = await _taskGroupRepository.WithDetailsAsync();
        taskGroupQuery = taskGroupQuery.Where(tg => tg.StartDate <= today &&
                                                    (!tg.EndDate.HasValue || tg.EndDate.Value >= today));

        var totalCount = await AsyncExecuter.CountAsync(taskGroupQuery);

        var result = await AsyncExecuter.ToListAsync(taskGroupQuery.PageBy(skipCount,maxResultCount));

        return (result, totalCount);
    }

    /// <summary>
    /// Gets all task groups owned by a specific user.
    /// </summary>
    public async Task<(List<TaskGroup> Items, int TotalCount)> GetUserOwnedTaskGroupsAsync(Guid userId, int skipCount,
        int maxResultCount)
    {
        var taskGroupQuery = await _taskGroupRepository.WithDetailsAsync();

        var totalCount = await AsyncExecuter.CountAsync(taskGroupQuery);


        var items = await AsyncExecuter.ToListAsync(taskGroupQuery.PageBy(skipCount, maxResultCount));

        return (items, totalCount);
    }


    public async Task<TaskGroupInvitation> CreateInvitationAsync(
        Guid taskGroupId,
        Guid createdByUserId,
        int expirationHours,
        int maxUses,
        UserTaskGroupRole defaultRole)
    {
        var taskGroup = await GetWithDetailsAsync(taskGroupId);

        // Validate that the task group is active
        var now = Clock.Now;
        if (taskGroup.EndDate.HasValue && taskGroup.EndDate.Value < now.Date)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.CannotGenerateInvitationForInactiveGroup);
        }

        // Validate that the user is the owner of the task group
        var userTaskGroup = taskGroup.UserTaskGroups.FirstOrDefault(utg => utg.UserId == createdByUserId);
        if (userTaskGroup == null || userTaskGroup.Role != UserTaskGroupRole.Owner)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserNotInGroup);
        }

        var token = _tokenGenerator.GenerateToken();
        var expirationDate = Clock.Now.AddHours(expirationHours);

        var invitation = new TaskGroupInvitation(
            GuidGenerator.Create(),
            taskGroupId,
            token,
            expirationDate,
            createdByUserId,
            maxUses,
            defaultRole);

        return await _invitationRepository.InsertAsync(invitation);
    }

    public async Task<TaskGroupInvitation> GetInvitationByTokenAsync(string invitationToken)
    {
        var invitation = await _invitationRepository.FirstOrDefaultAsync(i => i.InvitationToken == invitationToken);

        if (invitation == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvitationNotFound);
        }

        return invitation;
    }

    public async Task<UserTaskGroup> JoinTaskGroupByInvitationAsync(
        string invitationToken,
        Guid userId)
    {
        var invitation = await GetInvitationByTokenAsync(invitationToken);

        // Validate the invitation
        invitation.ValidateForUse();

        // Check if user is already in the group
        var taskGroup = await GetWithDetailsAsync(invitation.TaskGroupId);
        if (taskGroup.UserTaskGroups.Any(utg => utg.UserId == userId))
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserAlreadyInGroup);
        }

        // Add user to the group
        var userTaskGroup = await AddUserToGroupAsync(invitation.TaskGroupId, userId, invitation.DefaultRole);

        // Increment invitation usage
        invitation.IncrementUses();
        await _invitationRepository.UpdateAsync(invitation);

        return userTaskGroup;
    }

    public async Task<List<TaskGroupInvitation>> GetTaskGroupInvitationsAsync(Guid taskGroupId)
    {
        var invitations = await _invitationRepository.GetListAsync(i => i.TaskGroupId == taskGroupId);
        return invitations.OrderByDescending(i => i.CreationTime).ToList();
    }

    public async Task DeleteInvitationAsync(Guid invitationId, Guid requestingUserId)
    {
        var invitation = await _invitationRepository.GetAsync(invitationId);

        // Validate that the requesting user is the owner of the task group
        var taskGroup = await GetWithDetailsAsync(invitation.TaskGroupId);
        var userTaskGroup = taskGroup.UserTaskGroups.FirstOrDefault(utg => utg.UserId == requestingUserId);
        if (userTaskGroup == null || userTaskGroup.Role != UserTaskGroupRole.Owner)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserNotInGroup);
        }

        await _invitationRepository.DeleteAsync(invitation);
    }
}