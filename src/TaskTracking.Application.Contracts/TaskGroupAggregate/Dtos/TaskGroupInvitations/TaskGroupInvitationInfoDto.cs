using System;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;

/// <summary>
/// DTO containing information about a task group invitation for public access.
/// </summary>
public class TaskGroupInvitationInfoDto
{
    /// <summary>
    /// The invitation code.
    /// </summary>
    public string InvitationCode { get; set; } = null!;

    /// <summary>
    /// The title of the task group.
    /// </summary>
    public string TaskGroupTitle { get; set; } = null!;

    /// <summary>
    /// The description of the task group.
    /// </summary>
    public string TaskGroupDescription { get; set; } = null!;

    /// <summary>
    /// The name of the user who created the invitation.
    /// </summary>
    public string InvitedByUserName { get; set; } = null!;

    /// <summary>
    /// The expiration date of the invitation.
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Whether the invitation is still valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Whether the invitation has expired.
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// Whether the invitation has been used.
    /// </summary>
    public bool IsUsed { get; set; }
}
