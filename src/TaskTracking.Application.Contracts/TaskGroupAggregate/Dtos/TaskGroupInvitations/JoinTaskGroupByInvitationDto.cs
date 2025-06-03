using System.ComponentModel.DataAnnotations;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;

public class JoinTaskGroupByInvitationDto
{
    /// <summary>
    /// The invitation token to use for joining the task group.
    /// </summary>
    [Required]
    public string InvitationToken { get; set; }
}
