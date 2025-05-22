using System;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

public class UserTaskProgressDto : AuditedEntityDto<Guid>
{
    public int ProgressPercentage { get; set; }
    public string Notes { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public bool IsCompleted { get; set; }
}