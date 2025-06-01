using System;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TaskTracking.TaskGroupAggregate;

/// <summary>
///     Application service interface for managing task items.
/// </summary>
public interface ITaskItemAppService : IApplicationService
{
    Task<TaskItemDto> GetAsync(Guid id);
    Task<PagedResultDto<TaskItemDto>> GetListAsync(PagedResultRequestDto input);
    /// <summary>
    ///     Gets all tasks for a specific task group.
    /// </summary>
    Task<PagedResultDto<TaskItemDto>> GetTasksForGroupAsync(Guid taskGroupId, PagedResultRequestDto input);

    /// <summary>
    ///     Gets all tasks due today for the current user.
    /// </summary>
    Task<PagedResultDto<TaskItemDto>> GetMyTasksDueTodayAsync(PagedResultRequestDto input);

}