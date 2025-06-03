using Volo.Abp.DependencyInjection;

namespace TaskTracking.TaskGroupAggregate.TaskGroupInvitations;

public interface ITaskGroupInvitationTokenGenerator : ITransientDependency
{
    /// <summary>
    /// Generates a cryptographically secure random token for task group invitations.
    /// </summary>
    /// <returns>A URL-safe base64 encoded token</returns>
    string GenerateToken();
}
