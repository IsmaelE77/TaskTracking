using System;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace TaskTracking.TaskGroupAggregate.TaskGroupInvitations;

public class TaskGroupInvitation : FullAuditedEntity<Guid>, IHaveTaskGroup
{
    public Guid TaskGroupId { get; private set; }
    public string InvitationToken { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public int MaxUses { get; private set; }
    public int CurrentUses { get; private set; }
    public UserTaskGroupRole DefaultRole { get; private set; }

    #region Navigation Properties

    public TaskGroup TaskGroup { get; private set; }
    public IdentityUser CreatedByUser { get; private set; }

    #endregion

    private TaskGroupInvitation()
    {
        // Required by EF Core
    }

    internal TaskGroupInvitation(
        Guid id,
        Guid taskGroupId,
        string invitationToken,
        DateTime expirationDate,
        Guid createdByUserId,
        int maxUses = TaskGroupInvitationConsts.DefaultMaxUses,
        UserTaskGroupRole defaultRole = UserTaskGroupRole.Subscriber) : base(id)
    {
        TaskGroupId = taskGroupId;
        SetInvitationToken(invitationToken);
        SetExpirationDate(expirationDate);
        CreatedByUserId = createdByUserId;
        SetMaxUses(maxUses);
        CurrentUses = 0;
        DefaultRole = defaultRole;
    }

    internal void SetInvitationToken(string token)
    {
        InvitationToken = Check.NotNullOrWhiteSpace(token, nameof(token), TaskGroupInvitationConsts.TokenLength);
    }

    internal void SetExpirationDate(DateTime expirationDate)
    {
        if (expirationDate <= DateTime.UtcNow)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvalidDateRange);
        }

        ExpirationDate = expirationDate;
    }

    internal void SetMaxUses(int maxUses)
    {
        if (maxUses < 0 || maxUses > TaskGroupInvitationConsts.MaxAllowedUses)
        {
            throw new ArgumentOutOfRangeException(nameof(maxUses), 
                $"Max uses must be between 0 and {TaskGroupInvitationConsts.MaxAllowedUses}");
        }

        MaxUses = maxUses;
    }

    public bool IsValid()
    {
        return !IsExpired() && !IsMaxUsesReached();
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpirationDate;
    }

    public bool IsMaxUsesReached()
    {
        return MaxUses > 0 && CurrentUses >= MaxUses;
    }

    internal void IncrementUses()
    {
        if (IsMaxUsesReached())
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvitationMaxUsesReached);
        }

        CurrentUses++;
    }

    internal void ValidateForUse()
    {
        if (IsExpired())
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvitationExpired);
        }

        if (IsMaxUsesReached())
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvitationMaxUsesReached);
        }
    }
}
