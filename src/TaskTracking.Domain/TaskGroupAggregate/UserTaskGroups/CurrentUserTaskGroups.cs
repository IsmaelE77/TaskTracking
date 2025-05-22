using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace TaskTracking.TaskGroupAggregate.UserTaskGroups;

public class CurrentUserTaskGroups : DomainService

{
    private ICurrentUser CurrentUser =>
        LazyServiceProvider.LazyGetRequiredService<ICurrentUser>();

    private IRepository<UserTaskGroup, Guid> UserTaskGroupRepository =>
        LazyServiceProvider.LazyGetRequiredService<IRepository<UserTaskGroup, Guid>>();

    private IDistributedCache<List<Guid>> DistributedCache =>
        LazyServiceProvider.LazyGetRequiredService<IDistributedCache<List<Guid>>>();


    public List<Guid> GetAccessibleTaskGroupIds()
    {
        if (CurrentUser.Id == null)
        {
            return [];
        }

        var cacheKey = BuildCacheKey(CurrentUser.Id.Value);

        return DistributedCache.GetOrAdd(cacheKey, () =>
            {
                var queryable = UserTaskGroupRepository.GetQueryableAsync().GetAwaiter().GetResult();

                return queryable
                    .Where(utg => utg.UserId == CurrentUser.Id.Value)
                    .Select(utg => utg.TaskGroupId)
                    .ToList();
            },
            () => new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(6)
            })!;
    }

    private static string BuildCacheKey(Guid userId)
    {
        return $"UserTaskGroups:{userId}";
    }

    public bool HasAccessToTaskGroup(Guid taskGroupId)
    {
        return GetAccessibleTaskGroupIds().Contains(taskGroupId);
    }
}