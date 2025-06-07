using System.Linq;
using AutoMapper;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace TaskTracking.TaskGroupAggregate;

public class TaskGroupDtoMappingAction : IMappingAction<TaskGroup, TaskGroupDto>, ITransientDependency
{
    private readonly ICurrentUser _currentUser;

    public TaskGroupDtoMappingAction(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void Process(TaskGroup source, TaskGroupDto destination, ResolutionContext context)
    {
        destination.CurrentUserRole = UserTaskGroupRole.Subscriber;

        if (_currentUser.Id == null)
        {
            return;
        }

        var userTaskGroup = source.UserTaskGroups.FirstOrDefault(utg => utg.UserId == _currentUser.Id);

        if (userTaskGroup != null)
        {
            destination.CurrentUserRole = userTaskGroup.Role;
        }

    }
}