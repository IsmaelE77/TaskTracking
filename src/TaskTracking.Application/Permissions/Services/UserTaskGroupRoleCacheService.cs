using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace TaskTracking.Permissions.Services;

public class UserTaskGroupRoleCacheService : IUserTaskGroupRoleCacheService, ITransientDependency
{
    private readonly IReadOnlyRepository<UserTaskGroup, Guid> _userTaskGroupRepository;
    private readonly IDistributedCache<UserTaskGroupWrap> _cache;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTaskGroupRoleCacheService(IReadOnlyRepository<UserTaskGroup, Guid> userTaskGroupRepository, IDistributedCache<UserTaskGroupWrap> cache, ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor)
    {
        _userTaskGroupRepository = userTaskGroupRepository;
        _cache = cache;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    private const string CacheKeyFormat = "UserTaskGroup{0}_{1}";


    public async Task<UserTaskGroupWrap?> GetAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var idValue = httpContext.GetRouteValue("id");

        if (_currentUser.Id.HasValue is false)
        {
            return null;
        }

        var userId = _currentUser.Id!.Value;


        if (idValue == null)
        {
            return null;
        }

        if (!Guid.TryParse(idValue.ToString(), out var taskGroupId))
        {
            return null;
        }

        var cacheKey = string.Format(CacheKeyFormat, userId, taskGroupId);

        var cached = await _cache.GetAsync(cacheKey);

        if (cached != null)
        {
            return cached;
        }

        var userTaskGroup = await _userTaskGroupRepository.FirstOrDefaultAsync(x =>
            x.TaskGroupId == taskGroupId &&
            x.UserId == userId
        );

        if (userTaskGroup == null)
        {
            return null;
        }

        var wrap = new UserTaskGroupWrap
        {
            Role = userTaskGroup.Role
        };

        await _cache.SetAsync(cacheKey, wrap, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });

        return wrap;
        
    }

    public async Task ClearAsync(Guid userId, Guid taskGroupId)
    {
        var cacheKey = string.Format(CacheKeyFormat, userId, taskGroupId);
        await _cache.RemoveAsync(cacheKey);
    }
}