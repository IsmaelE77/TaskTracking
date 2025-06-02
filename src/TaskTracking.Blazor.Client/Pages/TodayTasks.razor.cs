using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskItems;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.Blazor.Client.Pages;

public partial class TodayTasks
{
    [Inject] private ITaskItemAppService TaskItemAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private List<TaskItemDto> Tasks { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsLoadingMore { get; set; } = false;
    private bool HasMoreData { get; set; } = true;
    private int CurrentPage { get; set; } = 0;
    private int TotalTasksCount { get; set; } = 0;
    private const int PageSize = 20;

    // Filter properties
    private string SearchText { get; set; } = string.Empty;
    private TaskTypeFilter TaskTypeFilter { get; set; } = TaskTypeFilter.All;

    // Statistics
    private int CompletedTasksCount => Tasks.Count(t => t.IsCompleted);
    private int PendingTasksCount => Tasks.Count(t => !t.IsCompleted);

    protected override async Task OnInitializedAsync()
    {
        await LoadTasks();
    }

    private async Task LoadTasks()
    {
        try
        {
            IsLoading = true;
            CurrentPage = 0;
            Tasks.Clear();

            var input = new GetMyTasksDueTodayInput
            {
                SkipCount = 0,
                MaxResultCount = PageSize,
                SearchText = SearchText,
                TaskTypeFilter = TaskTypeFilter
            };

            var result = await TaskItemAppService.GetMyTasksDueTodayAsync(input);
            Tasks = result.Items.ToList();
            TotalTasksCount = (int)result.TotalCount;
            HasMoreData = Tasks.Count < TotalTasksCount;
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMoreTasks()
    {
        if (IsLoadingMore || !HasMoreData) return;

        try
        {
            IsLoadingMore = true;
            CurrentPage++;

            var input = new GetMyTasksDueTodayInput
            {
                SkipCount = CurrentPage * PageSize,
                MaxResultCount = PageSize,
                SearchText = SearchText,
                TaskTypeFilter = TaskTypeFilter
            };

            var result = await TaskItemAppService.GetMyTasksDueTodayAsync(input);
            Tasks.AddRange(result.Items);
            HasMoreData = Tasks.Count < TotalTasksCount;
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private async Task RefreshTasks()
    {
        await LoadTasks();
        Snackbar.Add(L["TasksRefreshed"], Severity.Success);
    }



    private async Task OnSearchKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await LoadTasks();
        }
    }

    private async Task OnSearchChanged()
    {
        await LoadTasks();
    }

    private async Task OnTaskTypeFilterChanged(TaskTypeFilter newValue)
    {
        TaskTypeFilter = newValue;
        await LoadTasks();
    }

    private async Task ClearFilters()
    {
        SearchText = string.Empty;
        TaskTypeFilter = TaskTypeFilter.All;
        await LoadTasks();
    }

    private async Task OnTaskDeleted()
    {
        await RefreshTasks();
    }

    private async Task OnTaskUpdated()
    {
        await RefreshTasks();
    }

    private string GetEmptyStateIcon()
    {
        if (TotalTasksCount == 0 && TaskTypeFilter == TaskTypeFilter.All && string.IsNullOrWhiteSpace(SearchText))
        {
            return Icons.Material.Filled.AssignmentTurnedIn;
        }

        return Icons.Material.Filled.Search;
    }

    private string GetEmptyStateTitle()
    {
        if (TotalTasksCount == 0 &&  TaskTypeFilter == TaskTypeFilter.All && string.IsNullOrWhiteSpace(SearchText))
        {
            return L["NoTasksDueToday"];
        }

        return L["NoTasksFound"];
    }

    private string GetEmptyStateDescription()
    {
        if (TotalTasksCount == 0 && TaskTypeFilter == TaskTypeFilter.All && string.IsNullOrWhiteSpace(SearchText))
        {
            return L["NoTasksDueTodayDescription"];
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            return L["NoTasksMatchSearch"];
        }

        return L["NoTasksFoundDescription"];
    }

    private new async Task HandleErrorAsync(Exception ex)
    {
        Console.WriteLine($"Error loading today's tasks: {ex.Message}");
        Snackbar.Add(L["ErrorLoadingTasks"], Severity.Error);
    }
}