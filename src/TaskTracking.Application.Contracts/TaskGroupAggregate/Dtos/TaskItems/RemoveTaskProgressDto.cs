using System;
using System.ComponentModel.DataAnnotations;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

public class RemoveTaskProgressDto
{
    [Required]
    public Guid TaskItemId { get; set; }
    [Required]
    public DateOnly Date { get; set; }
}