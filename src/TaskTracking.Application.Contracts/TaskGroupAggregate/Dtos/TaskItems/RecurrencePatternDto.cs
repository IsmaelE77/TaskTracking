using System;
using System.Collections.Generic;
using TaskTracking.TaskGroupAggregate.TaskItems;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

/// <summary>
///     DTO for RecurrencePattern entity.
/// </summary>
public class RecurrencePatternDto : EntityDto<Guid>
{
    /// <summary>
    ///     The type of recurrence (daily, weekly, monthly).
    /// </summary>
    public RecurrenceType RecurrenceType { get; set; }

    /// <summary>
    ///     The interval of recurrence (e.g., every 2 days, every 3 weeks).
    /// </summary>
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
    public int? Occurrences { get; set; }

    /// <summary>
    ///     The ID of the task this recurrence pattern belongs to.
    /// </summary>
    public Guid TaskItemId { get; set; }
}