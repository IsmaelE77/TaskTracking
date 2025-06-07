using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.Blazor.Client.Components;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Severity = MudBlazor.Severity;

namespace TaskTracking.Blazor.Client.Pages;

public partial class ViewTaskItem
{
    [Parameter] public Guid TaskGroupId { get; set; }
    [Parameter] public Guid TaskItemId { get; set; }
    [Inject] private ITaskItemAppService ItemAppService { get; set; } = null!;
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private UserTaskGroupRole UserTaskGroupRole { get; set; }

    private TaskItemDto? TaskItem { get; set; }
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadTaskItemAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadTaskItemAsync()
    {
        try
        {
            TaskItem = await ItemAppService.GetAsync(TaskItemId);
            UserTaskGroupRole = await TaskGroupAppService.GetUserRoleAsync(TaskGroupId);
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorLoadingTask"], Severity.Error);
            Console.WriteLine($"Error loading task: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string GetTaskTypeIcon()
    {
        if (TaskItem == null) return Icons.Material.Filled.Assignment;
        
        return TaskItem.TaskType switch
        {
            TaskType.OneTime => Icons.Material.Filled.Event,
            TaskType.Recurring => Icons.Material.Filled.Repeat,
            _ => Icons.Material.Filled.Assignment
        };
    }

    private string GetTaskTypeDescription()
    {
        if (TaskItem == null) return "";
        
        return TaskItem.TaskType switch
        {
            TaskType.OneTime => L["OneTimeTaskDescription"],
            TaskType.Recurring => L["RecurringTaskDescription"],
            _ => ""
        };
    }

    private bool IsOverdue()
    {
        return TaskItem != null && 
               !TaskItem.IsCompleted && 
               TaskItem.EndDate.HasValue && 
               TaskItem.EndDate.Value.Date < DateTime.Today;
    }

    private bool IsDueToday()
    {
        return TaskItem?.IsDueToday ?? false;
    }

    private string GetIntervalText()
    {
        if (TaskItem?.RecurrencePattern == null) return "";
        
        var pattern = TaskItem.RecurrencePattern;
        return pattern.RecurrenceType switch
        {
            RecurrenceType.Daily => string.Format(L["TaskRecurrence_EveryNDays"], pattern.Interval),
            RecurrenceType.Weekly => string.Format(L["TaskRecurrence_EveryNWeeks"], pattern.Interval),
            RecurrenceType.Monthly => string.Format(L["TaskRecurrence_EveryNMonths"], pattern.Interval),
            _ => pattern.Interval.ToString()
        };
    }

    private int GetProgressPercentage()
    {
        if (TaskItem?.UserTaskProgressDtos == null || !TaskItem.UserTaskProgressDtos.Any()) 
            return 0;
        
        var completedDays = TaskItem.UserTaskProgressDtos.Count(p => p.ProgressPercentage == 100);
        var totalDays = TaskItem.UserTaskProgressDtos.Count;
        
        return totalDays > 0 ? (int)Math.Round((double)completedDays / totalDays * 100) : 0;
    }

    private Color GetProgressColor()
    {
        var percentage = GetProgressPercentage();
        return percentage switch
        {
            >= 80 => Color.Success,
            >= 50 => Color.Warning,
            _ => Color.Primary
        };
    }

    private string GetProgressDescription()
    {
        if (TaskItem?.UserTaskProgressDtos == null || !TaskItem.UserTaskProgressDtos.Any())
            return L["NoProgressRecorded"];

        var completedDays = TaskItem.UserTaskProgressDtos.Count(p => p.ProgressPercentage == 100);
        var totalDays = TaskItem.UserTaskProgressDtos.Count;

        return string.Format(L["ProgressDescription"], completedDays, totalDays);
    }

    private async Task RecordProgress()
    {
        if (TaskItem == null) return;

        try
        {
            var taskProgressDetail = await TaskGroupAppService.GetTaskProgressDetailAsync(TaskGroupId, TaskItemId);

            var parameters = new DialogParameters
            {
                ["TaskGroupId"] = TaskGroupId,
                ["TaskProgressDetail"] = taskProgressDetail
            };

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                CloseButton = true,
            };

            var result = await DialogService.ShowAsync<ProgressRecordingDialog>(L["RecordProgress"], parameters, options);

            var dialogResult = await result.Result;

            if (dialogResult is { Canceled: false })
            {
                await LoadTaskItemAsync();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorLoadingProgressDetail"], Severity.Error);
            Console.WriteLine($"Error loading progress detail: {ex.Message}");
        }
    }
}