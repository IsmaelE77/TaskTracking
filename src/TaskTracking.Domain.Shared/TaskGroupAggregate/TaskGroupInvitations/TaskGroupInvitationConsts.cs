namespace TaskTracking.TaskGroupAggregate.TaskGroupInvitations;

public static class TaskGroupInvitationConsts
{
    /// <summary>
    /// Length of the invitation token.
    /// </summary>
    public const int TokenLength = 32;

    /// <summary>
    /// Default expiration time for invitations in hours.
    /// </summary>
    public const int DefaultExpirationHours = 168; // 7 days

    /// <summary>
    /// Maximum expiration time for invitations in hours.
    /// </summary>
    public const int MaxExpirationHours = 720; // 30 days

    /// <summary>
    /// Default maximum number of uses for an invitation (0 = unlimited).
    /// </summary>
    public const int DefaultMaxUses = 0;

    /// <summary>
    /// Maximum allowed uses for an invitation.
    /// </summary>
    public const int MaxAllowedUses = 1000;
}
