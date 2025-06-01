using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskTracking.TaskGroupAggregate.TaskItems;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;


public class CreateRecurrencePatternDto
{
    public RecurrenceType RecurrenceType { get; set; }
    public int Interval { get; set; }
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
    public DateTime? EndDate { get; set; }
    public int? Occurrences { get; set; }
}