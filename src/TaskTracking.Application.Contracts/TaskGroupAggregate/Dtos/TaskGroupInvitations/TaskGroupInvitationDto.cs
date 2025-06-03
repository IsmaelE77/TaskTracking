using System;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;

/// <summary>
/// DTO for TaskGroupInvitation entity.
/// </summary>
public class TaskGroupInvitationDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// The ID of the task group this invitation is for.
    /// </summary>
    public Guid TaskGroupId { get; set; }

    /// <summary>
    /// The unique invitation code.
    /// </summary>
    public string InvitationCode { get; set; } = null!;

    /// <summary>
    /// The expiration date of the invitation.
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Whether the invitation has been used.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// The ID of the user who used the invitation.
    /// </summary>
    public Guid? UsedByUserId { get; set; }

    /// <summary>
    /// The username of the user who used the invitation.
    /// </summary>
    public string? UsedByUserName { get; set; }

    /// <summary>
    /// When the invitation was used.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Whether the invitation is still valid (not used and not expired).
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Whether the invitation has expired.
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// The full invitation URL.
    /// </summary>
    public string? InvitationUrl { get; set; }
}
