namespace TaskTracking.TaskGroupAggregate.UserTaskGroups;

public enum UserTaskGroupRole
{
    /// <summary>
    /// A subscriber can view and complete tasks in the group.
    /// </summary>
    Subscriber = 1,

    /// <summary>
    /// A co-owner can manage tasks and add/remove subscribers.
    /// </summary>
    CoOwner = 2,

    /// <summary>
    /// The owner has full control over the group.
    /// </summary>
    Owner = 3
}