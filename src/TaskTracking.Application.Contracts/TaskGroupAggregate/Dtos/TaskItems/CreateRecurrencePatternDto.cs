using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskTracking.TaskGroupAggregate.TaskItems;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

/// <summary>
///     DTO for creating a recurrence pattern.
/// </summary>
public class CreateRecurrencePatternDto
{
    /// <summary>
    ///     The type of recurrence (daily, weekly, monthly).
    /// </summary>
    [Required]
    public RecurrenceType RecurrenceType { get; set; }

    /// <summary>
    ///     The interval of recurrence (e.g., every 2 days, every 3 weeks).
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int Interval { get; set; }

    /// <summary>
    ///     The days of the week on which the task recurs (for weekly recurrence).
    /// </summary>
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();

    /// <summary>
    ///     The end date of the recurrence. Null means no end date.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    ///     The number of occurrences. Null means no limit.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? Occurrences { get; set; }
}