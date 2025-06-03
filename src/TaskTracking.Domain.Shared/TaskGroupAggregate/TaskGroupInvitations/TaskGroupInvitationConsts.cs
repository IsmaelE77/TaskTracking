namespace TaskTracking.TaskGroupAggregate.TaskGroupInvitations;

public static class TaskGroupInvitationConsts
{
    /// <summary>
    /// Length of the invitation code.
    /// </summary>
    public const int InvitationCodeLength = 32;

    /// <summary>
    /// Default expiration time for invitations in hours.
    /// </summary>
    public const int DefaultExpirationHours = 168; // 7 days

    /// <summary>
    /// Maximum number of active invitations per task group.
    /// </summary>
    public const int MaxActiveInvitationsPerGroup = 10;
}
