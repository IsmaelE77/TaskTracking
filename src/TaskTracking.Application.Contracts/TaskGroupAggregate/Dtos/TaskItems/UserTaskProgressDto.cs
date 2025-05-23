using System;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

public class UserTaskProgressDto : AuditedEntityDto<Guid>
{
    public int ProgressPercentage { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}