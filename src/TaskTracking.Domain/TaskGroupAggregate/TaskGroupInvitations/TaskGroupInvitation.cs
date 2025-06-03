using System;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace TaskTracking.TaskGroupAggregate.TaskGroupInvitations;

public class TaskGroupInvitation : FullAuditedEntity<Guid>
{
    public Guid TaskGroupId { get; private set; }
    public string InvitationCode { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public bool IsUsed { get; private set; }
    public Guid? UsedByUserId { get; private set; }
    public DateTime? UsedAt { get; private set; }

    #region Navigation Properties

    public TaskGroup TaskGroup { get; private set; }
    public IdentityUser? UsedByUser { get; private set; }

    #endregion

    private TaskGroupInvitation()
    {
        // Required by EF Core
    }

    internal TaskGroupInvitation(
        Guid id,
        Guid taskGroupId,
        string invitationCode,
        DateTime expirationDate) : base(id)
    {
        TaskGroupId = taskGroupId;
        SetInvitationCode(invitationCode);
        SetExpirationDate(expirationDate);
        IsUsed = false;
    }

    internal void SetInvitationCode(string invitationCode)
    {
        InvitationCode = Check.NotNullOrWhiteSpace(
            invitationCode, 
            nameof(invitationCode), 
            TaskGroupInvitationConsts.InvitationCodeLength);
    }

    internal void SetExpirationDate(DateTime expirationDate)
    {
        if (expirationDate <= DateTime.UtcNow)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvalidDateRange);
        }

        ExpirationDate = expirationDate;
    }

    internal void MarkAsUsed(Guid userId)
    {
        if (IsUsed)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvitationAlreadyUsed);
        }

        if (DateTime.UtcNow > ExpirationDate)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvitationExpired);
        }

        IsUsed = true;
        UsedByUserId = userId;
        UsedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow <= ExpirationDate;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpirationDate;
    }
}
