using System;
using System.ComponentModel.DataAnnotations;
using TaskTracking.TaskGroupAggregate.TaskGroups;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;

/// <summary>
///     DTO for updating an existing task group.
/// </summary>
public class UpdateTaskGroupDto
{
    /// <summary>
    ///     The title of the task group.
    /// </summary>
    [Required]
    [StringLength(TaskGroupConsts.MaxTitleLength)]
    public string Title { get; set; }

    /// <summary>
    ///     The detailed description of the task group with Markdown support.
    /// </summary>
    [Required]
    [StringLength(TaskGroupConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>
    ///     The start date of the task group's validity period.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     The end date of the task group's validity period. Null means permanent.
    /// </summary>
    public DateTime? EndDate { get; set; }
}