using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.Blazor.Client.Pages;

public partial class AllTaskGroups
{
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private List<TaskGroupDto> TaskGroups { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsLoadingMore { get; set; } = false;
    private bool HasMoreData { get; set; } = true;
    private int TotalCount { get; set; } = 0;
    private int CurrentPage { get; set; } = 0;
    private const int PageSize = 12;

    // Filter properties
    private string SearchText { get; set; } = string.Empty;
    private TaskGroupStatusFilter StatusFilter { get; set; } = TaskGroupStatusFilter.All;
    private TaskGroupSortBy SortBy { get; set; } = TaskGroupSortBy.CreationTime;

    protected override async Task OnInitializedAsync()
    {
        await LoadTaskGroups();
    }

    private async Task LoadTaskGroups(bool append = false)
    {
        try
        {
            if (!append)
            {
                IsLoading = true;
                TaskGroups.Clear();
                CurrentPage = 0;
            }
            else
            {
                IsLoadingMore = true;
            }

            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = PageSize,
                SkipCount = CurrentPage * PageSize,
                
                Sorting = GetSortingString()
            };

            var result = await TaskGroupAppService.GetListAsync(input);
            
            if (append)
            {
                TaskGroups.AddRange(result.Items);
            }
            else
            {
                TaskGroups = result.Items.ToList();
            }

            TotalCount = (int)result.TotalCount;
            HasMoreData = TaskGroups.Count < TotalCount;
            CurrentPage++;

            // Apply client-side status filtering if needed
            if (StatusFilter != TaskGroupStatusFilter.All)
            {
                TaskGroups = FilterByStatus(TaskGroups).ToList();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorLoadingTaskGroups"], Severity.Error);
            Console.WriteLine($"Error loading task groups: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            IsLoadingMore = false;
        }
    }

    private async Task LoadMoreTaskGroups()
    {
        await LoadTaskGroups(append: true);
    }

    private async Task ApplyFilters()
    {
        await LoadTaskGroups();
    }

    private async Task OnSearchKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await ApplyFilters();
        }
    }

    private async Task ClearSearch()
    {
        SearchText = string.Empty;
        await ApplyFilters();
    }

    private async Task ClearFiltersAndReload()
    {
        SearchText = string.Empty;
        StatusFilter = TaskGroupStatusFilter.All;
        SortBy = TaskGroupSortBy.CreationTime;
        await LoadTaskGroups();
    }

    private string GetSortingString()
    {
        return SortBy switch
        {
            TaskGroupSortBy.Title => "Title",
            TaskGroupSortBy.StartDate => "StartDate",
            TaskGroupSortBy.Progress => "ProgressPercentageCompleted desc",
            _ => "CreationTime desc"
        };
    }

    private IEnumerable<TaskGroupDto> FilterByStatus(IEnumerable<TaskGroupDto> taskGroups)
    {
        return StatusFilter switch
        {
            TaskGroupStatusFilter.Active => taskGroups.Where(tg => !tg.IsCompleted && (!tg.EndDate.HasValue || tg.EndDate.Value >= DateTime.Now)),
            TaskGroupStatusFilter.Completed => taskGroups.Where(tg => tg.IsCompleted),
            TaskGroupStatusFilter.Expired => taskGroups.Where(tg => !tg.IsCompleted && tg.EndDate.HasValue && tg.EndDate.Value < DateTime.Now),
            _ => taskGroups
        };
    }
}

public enum TaskGroupStatusFilter
{
    All,
    Active,
    Completed,
    Expired
}

public enum TaskGroupSortBy
{
    CreationTime,
    Title,
    StartDate,
    Progress
}
