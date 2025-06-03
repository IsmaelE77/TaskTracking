using System.ComponentModel.DataAnnotations;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;

/// <summary>
/// DTO for creating a new task group invitation.
/// </summary>
public class CreateTaskGroupInvitationDto
{
    /// <summary>
    /// The expiration time for the invitation in hours. If not specified, uses the default.
    /// </summary>
    [Range(1, 8760, ErrorMessage = "Expiration hours must be between 1 and 8760 (1 year).")]
    public int? ExpirationHours { get; set; }
}
