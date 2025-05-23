using System;
using System.ComponentModel.DataAnnotations;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

/// <summary>
///     DTO for recording progress on a task.
/// </summary>
public class RecordTaskProgressDto
{
    /// <summary>
    ///     The ID of the task group the task belongs to.
    /// </summary>
    [Required]
    public Guid TaskGroupId { get; set; }
    
    /// <summary>
    ///     The ID of the task to record progress for.
    /// </summary>
    [Required]
    public Guid TaskItemId { get; set; }
    
    /// <summary>
    ///     The date for which progress is being recorded.
    /// </summary>
    [Required]
    public DateOnly Date { get; set; }
}
