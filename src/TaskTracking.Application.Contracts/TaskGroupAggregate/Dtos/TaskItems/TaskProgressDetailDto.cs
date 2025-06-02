using System;
using System.Collections.Generic;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

/// <summary>
/// DTO containing detailed progress information for a task, including due dates and completion status.
/// </summary>
public class TaskProgressDetailDto
{
    /// <summary>
    /// The task item information.
    /// </summary>
    public TaskItemDto TaskItem { get; set; } = null!;
    
    /// <summary>
    /// The current user's progress for this task.
    /// </summary>
    public UserTaskProgressDto? UserProgress { get; set; }
    
    /// <summary>
    /// List of all due dates for this task (for recurring tasks).
    /// </summary>
    public List<DateOnly> DueDates { get; set; } = new();
    
    /// <summary>
    /// List of dates when progress was recorded.
    /// </summary>
    public List<DateOnly> CompletedDates { get; set; } = new();
    
    /// <summary>
    /// Total number of times this task is due.
    /// </summary>
    public int TotalDueCount { get; set; }
    
    /// <summary>
    /// Number of times progress has been recorded.
    /// </summary>
    public int CompletedCount { get; set; }
    
    /// <summary>
    /// Whether the task is fully completed.
    /// </summary>
    public bool IsFullyCompleted { get; set; }
    
    /// <summary>
    /// Whether the task is due today.
    /// </summary>
    public bool IsDueToday { get; set; }
    
    /// <summary>
    /// Whether progress can be recorded for today.
    /// </summary>
    public bool CanRecordToday { get; set; }
}
