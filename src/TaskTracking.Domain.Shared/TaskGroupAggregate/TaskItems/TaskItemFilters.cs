namespace TaskTracking.TaskGroupAggregate.TaskItems;

/// <summary>
/// Enumeration for filtering tasks by type
/// </summary>
public enum TaskTypeFilter
{
    /// <summary>
    /// Show all task types
    /// </summary>
    All = 0,

    /// <summary>
    /// Show only one-time tasks
    /// </summary>
    OneTime = 1,

    /// <summary>
    /// Show only recurring tasks
    /// </summary>
    Recurring = 2
}