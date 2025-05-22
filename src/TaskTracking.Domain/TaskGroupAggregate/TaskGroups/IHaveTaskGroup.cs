using System;

namespace TaskTracking.TaskGroupAggregate.TaskGroups;

public interface IHaveTaskGroup
{
    public Guid TaskGroupId { get; }
}