using System;

namespace TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;


public class CreateTaskGroupDto
{
    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}