using System;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;

public class TaskGroupInvitationDto : FullAuditedEntityDto<Guid>
{
    public Guid TaskGroupId { get; set; }
    public string TaskGroupTitle { get; set; }
    public string InvitationToken { get; set; }
    public DateTime ExpirationDate { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; }
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public UserTaskGroupRole DefaultRole { get; set; }
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
    public bool IsMaxUsesReached { get; set; }
    public string InvitationUrl { get; set; }
}
