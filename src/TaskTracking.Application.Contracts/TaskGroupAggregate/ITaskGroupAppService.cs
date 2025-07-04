using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TaskTracking.TaskGroupAggregate;

/// <summary>
///     Application service interface for managing task groups.
/// </summary>
public interface ITaskGroupAppService :
    ICrudAppService<
        TaskGroupDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateTaskGroupDto,
        UpdateTaskGroupDto>
{
    /// <summary>
    ///     Gets all task groups owned by the current user.
    /// </summary>
    Task<PagedResultDto<TaskGroupDto>> GetMyOwnedTaskGroupsAsync(PagedResultRequestDto input);

    /// <summary>
    ///     Gets all active task groups for the current user.
    /// </summary>
    Task<PagedResultDto<TaskGroupDto>> GetMyActiveTaskGroupsAsync(PagedResultRequestDto input);

    /// <summary>
    ///     Adds a user to a task group with the specified role.
    /// </summary>
    Task<UserTaskGroupDto> AddUserAsync(Guid id, Guid userId, UserTaskGroupRole role);

    /// <summary>
    ///     Removes a user from a task group.
    /// </summary>
    Task RemoveUserAsync(Guid id, Guid userId);

    /// <summary>
    ///     Changes the role of a user in a task group.
    /// </summary>
    Task<UserTaskGroupDto> UpdateUserRoleAsync(Guid id, Guid userId, UserTaskGroupRole newRole);

    /// <summary>
    ///     Gets all users in a task group.
    /// </summary>
    Task<List<UserTaskGroupDto>> GetTaskGroupUsersAsync(Guid id);

    /// <summary>
    ///     Searches for users that can be added to a task group.
    /// </summary>
    Task<PagedResultDto<UserSearchResultDto>> SearchUsersAsync(SearchUsersInput input);

    /// <summary>
    ///     Records progress for a task on a specific date.
    /// </summary>
    Task RecordTaskProgressAsync(Guid id, RecordTaskProgressDto input);

    /// <summary>
    ///     Removes progress for a task on a specific date.
    /// </summary>
    Task RemoveTaskProgressAsync(Guid id, RemoveTaskProgressDto input);

    /// <summary>
    ///     Gets detailed progress information for a specific task.
    /// </summary>
    Task<TaskProgressDetailDto> GetTaskProgressDetailAsync(Guid taskGroupId, Guid taskItemId);

    /// <summary>
    ///     Creates a new task item in the specified task group.
    /// </summary>
    Task<TaskItemDto> CreateTaskItemAsync(
        Guid id,
        CreateTaskItemDto input);

    /// <summary>
    ///     Updates an existing task item in the specified task group.
    /// </summary>
    Task<TaskItemDto> UpdateTaskItemAsync(
        Guid id,
        Guid itemTaskId,
        UpdateTaskItemDto input);

    /// <summary>
    ///     Deletes a task item from the specified task group.
    /// </summary>
    Task<TaskItemDto> DeleteTaskItemAsync(
        Guid id,
        Guid taskItemId);

    /// <summary>
    ///     Generates a new invitation link for the specified task group.
    /// </summary>
    Task<TaskGroupInvitationDto> GenerateInvitationLinkAsync(
        Guid id,
        CreateTaskGroupInvitationDto input);

    /// <summary>
    ///     Gets invitation details by token without requiring authentication.
    /// </summary>
    Task<TaskGroupInvitationDetailsDto> GetInvitationDetailsAsync(string invitationToken);

    /// <summary>
    ///     Allows a user to join a task group using an invitation token.
    /// </summary>
    Task<UserTaskGroupDto> JoinTaskGroupByInvitationAsync(JoinTaskGroupByInvitationDto input);

    /// <summary>
    ///     Gets all invitations for a specific task group.
    /// </summary>
    Task<List<TaskGroupInvitationDto>> GetTaskGroupInvitationsAsync(Guid id);

    /// <summary>
    ///     Deletes an invitation.
    /// </summary>
    Task DeleteInvitationAsync(Guid invitationId);

    Task<UserTaskGroupRole> GetUserRoleAsync(Guid id);

}