using System.ComponentModel.DataAnnotations;
using TaskTracking.TaskGroupAggregate.TaskItems;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

public class GetMyUpcomingTasksInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Search text to filter tasks by title or description
    /// </summary>
    [MaxLength(256)]
    public string? SearchText { get; set; }

    /// <summary>
    /// Filter tasks by task type
    /// </summary>
    public TaskTypeFilter TaskTypeFilter { get; set; } = TaskTypeFilter.All;

    /// <summary>
    /// Number of days to look ahead for upcoming tasks (default: 7 days)
    /// </summary>
    [Range(1, 365)]
    public int DaysAhead { get; set; } = 7;
}
