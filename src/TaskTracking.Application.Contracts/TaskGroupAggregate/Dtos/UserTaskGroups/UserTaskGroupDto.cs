using System;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;

/// <summary>
///     DTO for UserTaskGroup entity.
/// </summary>
public class UserTaskGroupDto : EntityDto<Guid>
{
    /// <summary>
    ///     The ID of the user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     The username of the user.
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    ///     The ID of the task group.
    /// </summary>
    public Guid TaskGroupId { get; set; }

    /// <summary>
    ///     The role of the user in the task group.
    /// </summary>
    public UserTaskGroupRole Role { get; set; }
}