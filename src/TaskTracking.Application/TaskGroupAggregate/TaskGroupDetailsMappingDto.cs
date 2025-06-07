using System.Linq;
using AutoMapper;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace TaskTracking.TaskGroupAggregate;

public class TaskGroupDetailsMappingDto : IMappingAction<TaskGroup, TaskGroupDetailsDto>, ITransientDependency
{
    private readonly ICurrentUser _currentUser;

    public TaskGroupDetailsMappingDto(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void Process(TaskGroup source, TaskGroupDetailsDto destination, ResolutionContext context)
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