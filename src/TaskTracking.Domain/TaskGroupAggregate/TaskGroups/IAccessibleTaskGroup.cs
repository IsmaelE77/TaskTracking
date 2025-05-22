using System;

namespace TaskTracking.TaskGroupAggregate.TaskGroups;

public interface IAccessibleTaskGroup
{
    Guid Id { get; }
}