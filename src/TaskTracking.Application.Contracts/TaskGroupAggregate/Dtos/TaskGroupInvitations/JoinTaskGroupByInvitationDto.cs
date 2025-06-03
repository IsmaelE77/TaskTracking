using System.ComponentModel.DataAnnotations;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;

/// <summary>
/// DTO for joining a task group using an invitation code.
/// </summary>
public class JoinTaskGroupByInvitationDto
{
    /// <summary>
    /// The invitation code to use for joining.
    /// </summary>
    [Required]
    [StringLength(32, MinimumLength = 32, ErrorMessage = "Invitation code must be exactly 32 characters.")]
    public string InvitationCode { get; set; } = null!;
}
