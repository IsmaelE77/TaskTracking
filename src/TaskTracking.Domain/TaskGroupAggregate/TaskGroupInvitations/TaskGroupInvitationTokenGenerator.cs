using System;
using System.Security.Cryptography;
using Volo.Abp.DependencyInjection;

namespace TaskTracking.TaskGroupAggregate.TaskGroupInvitations;

public class TaskGroupInvitationTokenGenerator : ITaskGroupInvitationTokenGenerator, ITransientDependency
{
    public string GenerateToken()
    {
        // Generate 24 random bytes (which will result in 32 characters when base64 encoded)
        var randomBytes = new byte[24];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert to URL-safe base64 string
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
