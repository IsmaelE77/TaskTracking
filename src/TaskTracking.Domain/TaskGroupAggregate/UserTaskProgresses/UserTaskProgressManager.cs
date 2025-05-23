using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;


namespace TaskTracking.TaskGroupAggregate.UserTaskProgresses;

public class UserTaskProgressManager : DomainService
{
    private readonly IReadOnlyRepository<UserTaskProgress, Guid> _userTaskProgressRepository;
    private readonly IReadOnlyRepository<UserTaskGroup, Guid> _userTaskGroupRepository;

    public UserTaskProgressManager(IReadOnlyRepository<UserTaskProgress, Guid> userTaskGroupRepository, IReadOnlyRepository<UserTaskGroup, Guid> userTaskGroupRepository1)
    {
        _userTaskProgressRepository = userTaskGroupRepository;
        _userTaskGroupRepository = userTaskGroupRepository1;
    }


    /// <summary>
    /// Gets all progress entries for a specific user.
    /// </summary>
    public async Task<List<UserTaskProgress>> GetUserProgressAsync(Guid userId)
    {
        return await _userTaskProgressRepository.GetListAsync(p => p.UserId == userId);
    }

    /// <summary>
    /// Gets all progress entries for a specific task.
    /// </summary>
    public async Task<List<UserTaskProgress>> GetTaskProgressAsync(Guid taskItemId)
    {
        return await _userTaskProgressRepository.GetListAsync(p => p.TaskItemId == taskItemId);
    }

}