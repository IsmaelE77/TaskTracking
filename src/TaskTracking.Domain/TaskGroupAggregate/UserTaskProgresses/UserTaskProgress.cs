using System;
using System.Collections.Generic;
using TaskTracking.TaskGroupAggregate.TaskItems;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace TaskTracking.TaskGroupAggregate.UserTaskProgresses;

public class UserTaskProgress : FullAuditedEntity<Guid>
{
    public Guid TaskItemId { get; private set; }
    public Guid UserId { get; private set; }
    public int ProgressPercentage { get; private set; }
    public DateTime LastUpdatedDate { get; private set; }
    public IReadOnlyCollection<ProgressEntry> ProgressEntries => _progressEntries.AsReadOnly();
    private readonly List<ProgressEntry> _progressEntries = [];


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
        int progressPercentage = 0) : base(id)
    {
        UserId = userId;
        TaskItemId = taskItemId;
        SetProgressPercentage(progressPercentage);
        LastUpdatedDate = DateTime.Now;
    }

    public void RecordProgress(DateOnly date)
    {
        var entry = new ProgressEntry(date);
        _progressEntries.Add(entry);
        LastUpdatedDate = DateTime.Now;
    }


    internal void SetProgressPercentage(int progressPercentage)
    {
        if (progressPercentage < 0 || progressPercentage > UserTaskProgressConsts.MaxProgressPercentage)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvalidProgressPercentage);
        }

        ProgressPercentage = progressPercentage;
    }

}