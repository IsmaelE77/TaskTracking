using System;
using System.Collections.Generic;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.TaskItems;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

/// <summary>
///     DTO for TaskItem entity.
/// </summary>
public class TaskItemDto : AuditedEntityDto<Guid>
{
    /// <summary>
    ///     The title of the task.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    ///     The description of the task.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    ///     The start date of the task.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     The end date of the task. Null for tasks without a deadline.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    ///     Whether the task is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    ///     The type of the task (one-time or recurring).
    /// </summary>
    public TaskType TaskType { get; set; }

    /// <summary>
    ///     The recurrence pattern for recurring tasks. Null for one-time tasks.
    /// </summary>
    public RecurrencePatternDto? RecurrencePattern { get; set; }

    /// <summary>
    ///     The ID of the task group this task belongs to.
    /// </summary>
    public Guid TaskGroupId { get; set; }

    public List<UserTaskProgressDto> UserTaskProgressDtos { get; set; } = new();

    public bool IsDueToday { get; set; }

}