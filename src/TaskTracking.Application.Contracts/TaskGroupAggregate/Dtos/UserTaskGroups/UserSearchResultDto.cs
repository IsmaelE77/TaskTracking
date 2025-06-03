using System;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;

/// <summary>
///     DTO for user search results when adding users to task groups.
/// </summary>
public class UserSearchResultDto : EntityDto<Guid>
{
    /// <summary>
    ///     The username of the user.
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    ///     The email of the user.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    ///     The name of the user.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Whether the user is already in the task group.
    /// </summary>
    public bool IsAlreadyInGroup { get; set; }
}
