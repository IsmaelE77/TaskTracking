using System;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;

/// <summary>
///     Input DTO for searching users to add to task groups.
/// </summary>
public class SearchUsersInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    ///     The search keyword to filter users by username, email, or name.
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    ///     The task group ID to exclude users already in the group.
    /// </summary>
    public Guid TaskGroupId { get; set; }
}
