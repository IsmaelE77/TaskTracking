using System;
using System.ComponentModel.DataAnnotations;
using TaskTracking.TaskGroupAggregate.TaskItems;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

/// <summary>
///     DTO for creating a new task item.
/// </summary>
public class CreateTaskItemDto
{
    /// <summary>
    ///     The title of the task.
    /// </summary>
    [Required]
    [StringLength(TaskItemConsts.MaxTitleLength)]
    public string Title { get; set; }

    /// <summary>
    ///     The description of the task.
    /// </summary>
    [Required]
    [StringLength(TaskItemConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>
    ///     The start date of the task.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     The end date of the task. Null for tasks without a deadline.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    ///     The type of the task (one-time or recurring).
    /// </summary>
    [Required]
    public TaskType TaskType { get; set; }

    /// <summary>
    ///     The recurrence pattern for recurring tasks. Required for recurring tasks.
    /// </summary>
    public CreateRecurrencePatternDto? RecurrencePattern { get; set; }

}