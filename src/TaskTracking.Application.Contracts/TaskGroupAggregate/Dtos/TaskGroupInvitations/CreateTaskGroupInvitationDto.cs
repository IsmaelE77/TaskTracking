using System;
using System.ComponentModel.DataAnnotations;
using TaskTracking.TaskGroupAggregate.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;

public class CreateTaskGroupInvitationDto
{
    /// <summary>
    /// Number of hours until the invitation expires. Default is 7 days (168 hours).
    /// </summary>
    [Range(1, TaskGroupInvitationConsts.MaxExpirationHours)]
    public int ExpirationHours { get; set; } = TaskGroupInvitationConsts.DefaultExpirationHours;

    /// <summary>
    /// Maximum number of times this invitation can be used. 0 means unlimited.
    /// </summary>
    [Range(0, TaskGroupInvitationConsts.MaxAllowedUses)]
    public int MaxUses { get; set; } = TaskGroupInvitationConsts.DefaultMaxUses;

    /// <summary>
    /// Default role for users joining through this invitation.
    /// </summary>
    public UserTaskGroupRole DefaultRole { get; set; } = UserTaskGroupRole.Subscriber;
}
