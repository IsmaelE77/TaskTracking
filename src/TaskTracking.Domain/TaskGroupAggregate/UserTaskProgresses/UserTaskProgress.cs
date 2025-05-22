using System;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace TaskTracking.TaskGroupAggregate.UserTaskProgresses;

public class UserTaskProgress : FullAuditedEntity<Guid>
{
    public Guid TaskItemId { get; private set; }
    public Guid UserId { get; private set; }
    public int ProgressPercentage { get; private set; }
    public string Notes { get; private set; }
    public DateTime LastUpdatedDate { get; private set; }
    public bool IsCompleted { get; private set; }

    #region Navigation Properties
    public IdentityUser User { get; private set; }
    public TaskItem TaskItem { get; private set; }

    #endregion

    private UserTaskProgress()
    {
        // Required by EF Core
    }

    internal UserTaskProgress(
        Guid id,
        Guid userId,
        Guid taskItemId,
        int progressPercentage = 0,
        string notes = "") : base(id)
    {
        UserId = userId;
        TaskItemId = taskItemId;
        SetProgressPercentage(progressPercentage);
        SetNotes(notes);
        LastUpdatedDate = DateTime.Now;
        IsCompleted = false;
    }

    internal void SetProgressPercentage(int progressPercentage)
    {
        if (progressPercentage < 0 || progressPercentage > UserTaskProgressConsts.MaxProgressPercentage)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvalidProgressPercentage);
        }

        ProgressPercentage = progressPercentage;
        LastUpdatedDate = DateTime.Now;
        
        // Automatically mark as completed if progress is 100%
        IsCompleted = progressPercentage == UserTaskProgressConsts.MaxProgressPercentage;
    }

    internal void SetNotes(string notes)
    {
        Notes = Check.Length(notes, nameof(notes), UserTaskProgressConsts.MaxNotesLength);
        LastUpdatedDate = DateTime.Now;
    }

    internal void MarkAsCompleted()
    {
        IsCompleted = true;
        ProgressPercentage = UserTaskProgressConsts.MaxProgressPercentage;
        LastUpdatedDate = DateTime.Now;
    }


    internal void MarkAsIncomplete()
    {
        IsCompleted = false;
        LastUpdatedDate = DateTime.Now;
    }
}