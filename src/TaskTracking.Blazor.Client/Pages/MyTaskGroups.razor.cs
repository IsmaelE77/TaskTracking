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
using Volo.Abp.Users;

namespace TaskTracking.Blazor.Client.Pages;

public partial class MyTaskGroups
{
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private List<TaskGroupDto> TaskGroups { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsLoadingMore { get; set; } = false;
    private bool HasMoreData { get; set; } = true;
    private int CurrentPage { get; set; } = 0;
    private const int PageSize = 12;

    // Filter properties
    private string SearchText { get; set; } = string.Empty;
    private MyGroupStatusFilter StatusFilter { get; set; } = MyGroupStatusFilter.All;
    private MyGroupSortBy SortBy { get; set; } = MyGroupSortBy.CreationTime;

    // Statistics
    private int TotalGroups => TaskGroups.Count;
    private int ActiveGroups => TaskGroups.Count(tg => !tg.IsCompleted && (!tg.EndDate.HasValue || tg.EndDate.Value >= DateTime.Now));
    private int CompletedGroups => TaskGroups.Count(tg => tg.IsCompleted);
    private int AverageProgress => TaskGroups.Any() ? (int)TaskGroups.Average(tg => tg.ProgressPercentageCompleted) : 0;

    private IEnumerable<TaskGroupDto> FilteredTaskGroups
    {
        get
        {
            var filtered = TaskGroups.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(tg => 
                    tg.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    tg.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Apply status filter
            filtered = StatusFilter switch
            {
                MyGroupStatusFilter.Active => filtered.Where(tg => !tg.IsCompleted && (!tg.EndDate.HasValue || tg.EndDate.Value >= DateTime.Now)),
                MyGroupStatusFilter.Completed => filtered.Where(tg => tg.IsCompleted),
                MyGroupStatusFilter.Expired => filtered.Where(tg => !tg.IsCompleted && tg.EndDate.HasValue && tg.EndDate.Value < DateTime.Now),
                _ => filtered
            };

            // Apply sorting
            filtered = SortBy switch
            {
                MyGroupSortBy.Title => filtered.OrderBy(tg => tg.Title),
                MyGroupSortBy.StartDate => filtered.OrderBy(tg => tg.StartDate),
                MyGroupSortBy.Progress => filtered.OrderByDescending(tg => tg.ProgressPercentageCompleted),
                _ => filtered.OrderByDescending(tg => tg.CreationTime)
            };

            return filtered;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadMyTaskGroups();
    }

    private async Task LoadMyTaskGroups()
    {
        await LoadMyTaskGroups(false);
    }


    private async Task LoadMyTaskGroups(bool append = false)
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

            var result = await TaskGroupAppService.GetMyOwnedTaskGroupsAsync(input);
            
            if (append)
            {
                TaskGroups.AddRange(result.Items);
            }
            else
            {
                TaskGroups = result.Items.ToList();
            }

            HasMoreData = result.Items.Count == PageSize;
            CurrentPage++;
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorLoadingMyGroups"], Severity.Error);
            Console.WriteLine($"Error loading my task groups: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            IsLoadingMore = false;
        }
    }

    private string GetSortingString()
    {
        return SortBy switch
        {
            MyGroupSortBy.Title => "Title",
            MyGroupSortBy.StartDate => "StartDate",
            MyGroupSortBy.Progress => "ProgressPercentageCompleted desc",
            _ => "CreationTime desc"
        };
    }

    private async Task LoadMoreTaskGroups()
    {
        await LoadMyTaskGroups(append: true);
    }

    private async Task ApplyFilters()
    {
        // Since we're filtering client-side, we just need to trigger a re-render
        StateHasChanged();
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
        StatusFilter = MyGroupStatusFilter.All;
        SortBy = MyGroupSortBy.CreationTime;
        await LoadMyTaskGroups();
    }
}

public enum MyGroupStatusFilter
{
    All,
    Active,
    Completed,
    Expired
}

public enum MyGroupSortBy
{
    CreationTime,
    Title,
    StartDate,
    Progress
}