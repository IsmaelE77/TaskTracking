using System;
using System.Collections.Generic;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;

public class TaskGroupDetailsDto
{
    /// <summary>
    ///     The title of the task group.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    ///     The detailed description of the task group with Markdown support.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    ///     The start date of the task group's validity period.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     The end date of the task group's validity period. Null means permanent.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    ///     Whether the task group is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    ///     The tasks associated with this task group.
    /// </summary>
    public List<TaskItemDto> Tasks { get; set; } = new();

    /// <summary>
    ///     The user subscriptions to this task group.
    /// </summary>
    public List<UserTaskGroupDto> UserTaskGroups { get; set; } = new();
}