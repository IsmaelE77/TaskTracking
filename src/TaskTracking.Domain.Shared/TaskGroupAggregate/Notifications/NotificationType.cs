namespace TaskTracking.TaskGroupAggregate.Notifications;

/// <summary>
/// Defines the types of notifications in the system.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Notification for a task due today.
    /// </summary>
    TaskDueToday = 1,
    
    /// <summary>
    /// Notification for a task that is overdue.
    /// </summary>
    TaskOverdue = 2,
    
    /// <summary>
    /// Notification for a task that is approaching its deadline.
    /// </summary>
    TaskApproachingDeadline = 3,
    
    /// <summary>
    /// Notification for a new task assigned to the user.
    /// </summary>
    NewTaskAssigned = 4,
    
    /// <summary>
    /// Notification for being added to a group.
    /// </summary>
    AddedToGroup = 5,
    
    /// <summary>
    /// Notification for a role change in a group.
    /// </summary>
    RoleChanged = 6
}
