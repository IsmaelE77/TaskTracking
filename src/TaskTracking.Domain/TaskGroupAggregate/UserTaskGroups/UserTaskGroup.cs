using System;
using System.Collections.Generic;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace TaskTracking.TaskGroupAggregate.UserTaskGroups;

public class UserTaskGroup : CreationAuditedEntity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid TaskGroupId { get; private set; }
    public UserTaskGroupRole Role { get; private set; }


    #region Navigation Properties

    public IdentityUser User { get; private set; }
    public TaskGroup TaskGroup { get; private set; }

    public IReadOnlyCollection<UserTaskProgress> UserProgresses => _userProgresses.AsReadOnly();
    private readonly List<UserTaskProgress> _userProgresses = new();

    #endregion


    private UserTaskGroup()
    {
        // Required by EF Core
    }

    internal UserTaskGroup(
        Guid id,
        Guid userId,
        Guid taskGroupId,
        UserTaskGroupRole role) : base(id)
    {
        UserId = userId;
        TaskGroupId = taskGroupId;
        Role = role;
    }

    internal void ChangeRole(UserTaskGroupRole newRole)
    {
        if (newRole == UserTaskGroupRole.Owner)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.CannotChangeToOwnerRole);
        }

        Role = newRole;
    }

    internal void AddUserProgress(UserTaskProgress progress)
    {
        _userProgresses.Add(progress);
    }

}