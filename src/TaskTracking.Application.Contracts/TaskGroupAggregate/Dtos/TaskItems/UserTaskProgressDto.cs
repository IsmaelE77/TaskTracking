using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

public class UserTaskProgressDto : AuditedEntityDto<Guid>
{
    public Guid TaskItemId { get; set; }
    public Guid UserId { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public List<ProgressEntryDto> ProgressEntries { get; set; } = new();
}