using System;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.Dtos;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
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
    ///     Marks a task group as completed.
    /// </summary>
    Task MarkAsCompletedAsync(Guid id);

    /// <summary>
    ///     Marks a task group as incomplete.
    /// </summary>
    Task MarkAsIncompleteAsync(Guid id);

    /// <summary>
    ///     Adds a user to a task group with the specified role.
    /// </summary>
    Task<UserTaskGroupDto> AddUserAsync(Guid taskGroupId, Guid userId, UserTaskGroupRole role);

    /// <summary>
    ///     Removes a user from a task group.
    /// </summary>
    Task RemoveUserAsync(Guid taskGroupId, Guid userId);

    /// <summary>
    ///     Changes the role of a user in a task group.
    /// </summary>
    Task<UserTaskGroupDto> ChangeUserRoleAsync(Guid taskGroupId, Guid userId, UserTaskGroupRole newRole);
}