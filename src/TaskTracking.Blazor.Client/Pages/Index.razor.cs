using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.Blazor.Client.Pages;

public partial class Index
{
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private PagedResultDto<TaskGroupDto>? TaskGroups { get; set; }
    private bool IsLoading { get; set; } = true;
    private bool IsLoadingMore { get; set; } = false;
    private int CurrentPage { get; set; } = 0;
    private const int PageSize = 12;

    protected override async Task OnInitializedAsync()
    {
        await LoadTaskGroups();
    }

    private async Task LoadTaskGroups()
    {
        try
        {
            IsLoading = true;
            var request = new PagedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = PageSize
            };

            TaskGroups = await TaskGroupAppService.GetMyActiveTaskGroupsAsync(request);
            CurrentPage = 1;
        }
        catch (Exception ex)
        {
            // Handle error - could show a snackbar or error message
            Console.WriteLine($"Error loading task groups: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMoreTaskGroups()
    {
        if (TaskGroups == null || IsLoadingMore) return;

        try
        {
            IsLoadingMore = true;
            var request = new PagedResultRequestDto
            {
                SkipCount = CurrentPage * PageSize,
                MaxResultCount = PageSize
            };

            var moreTaskGroups = await TaskGroupAppService.GetMyActiveTaskGroupsAsync(request);
            
            if (moreTaskGroups.Items.Count > 0)
            {
                var allItems = new List<TaskGroupDto>(TaskGroups.Items);
                allItems.AddRange(moreTaskGroups.Items);
                
                TaskGroups = new PagedResultDto<TaskGroupDto>(
                    moreTaskGroups.TotalCount,
                    allItems
                );
                
                CurrentPage++;
            }
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error loading more task groups: {ex.Message}");
        }
        finally
        {
            IsLoadingMore = false;
        }
    }
}
