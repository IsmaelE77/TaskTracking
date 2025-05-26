using System;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;

/// <summary>
///     DTO for TaskGroup entity.
/// </summary>
public class TaskGroupDto : AuditedEntityDto<Guid>
{
    /// <summary>
    ///     The title of the task group.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    ///     The detailed description of the task group with Markdown support.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    ///     The start date of the task group's validity period.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     The end date of the task group's validity period. Null means permanent.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    ///     Whether the task group is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    public double ProgressPercentageCompleted { get; set; }
}