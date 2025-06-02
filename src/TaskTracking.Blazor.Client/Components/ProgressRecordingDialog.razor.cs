using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskItems;

namespace TaskTracking.Blazor.Client.Components;

public partial class ProgressRecordingDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;
    
    [Parameter] public TaskProgressDetailDto? TaskProgressDetail { get; set; }
    [Parameter] public Guid TaskGroupId { get; set; }
    
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    
    private bool IsRecording { get; set; }
    private DateTime? SelectedDate { get; set; }

    protected override void OnInitialized()
    {
        // Set default selected date to today for one-time tasks
        if (TaskProgressDetail?.TaskItem.TaskType == TaskType.OneTime)
        {
            SelectedDate = DateTime.Today;
        }
    }

    private async Task RecordTodayProgress()
    {
        await RecordProgress(DateOnly.FromDateTime(DateTime.Today));
    }

    private async Task RecordSelectedDateProgress()
    {
        if (SelectedDate.HasValue)
        {
            await RecordProgress(DateOnly.FromDateTime(SelectedDate.Value));
        }
    }

    private async Task OnDateSelected(DateOnly date)
    {
        await RecordProgress(date);
    }

    private async Task RecordProgress(DateOnly date)
    {
        if (TaskProgressDetail == null) return;

        try
        {
            IsRecording = true;

            var recordDto = new RecordTaskProgressDto
            {
                TaskItemId = TaskProgressDetail.TaskItem.Id,
                Date = date
            };

            await TaskGroupAppService.RecordTaskProgressAsync(TaskGroupId, recordDto);

            Snackbar.Add(L["ProgressRecordedSuccessfully"], Severity.Success);

            // Refresh the task progress detail
            TaskProgressDetail = await TaskGroupAppService.GetTaskProgressDetailAsync(
                TaskGroupId, TaskProgressDetail.TaskItem.Id);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorRecordingProgress"], Severity.Error);
            Console.WriteLine($"Error recording progress: {ex.Message}");
        }
        finally
        {
            IsRecording = false;
        }
    }

    private string GetTaskTypeIcon()
    {
        return TaskProgressDetail?.TaskItem.TaskType switch
        {
            TaskType.OneTime => Icons.Material.Filled.Event,
            TaskType.Recurring => Icons.Material.Filled.Repeat,
            _ => Icons.Material.Filled.Assignment
        };
    }

    private Color GetProgressColor()
    {
        if (TaskProgressDetail == null) return Color.Primary;
        
        var percentage = GetProgressPercentage();
        return percentage switch
        {
            >= 80 => Color.Success,
            >= 50 => Color.Warning,
            _ => Color.Primary
        };
    }

    private double GetProgressPercentage()
    {
        if (TaskProgressDetail == null || TaskProgressDetail.TotalDueCount == 0) return 0;
        
        return (double)TaskProgressDetail.CompletedCount / TaskProgressDetail.TotalDueCount * 100;
    }

    private string GetProgressDescription()
    {
        if (TaskProgressDetail == null) return "";
        
        if (TaskProgressDetail.IsFullyCompleted)
        {
            return L["TaskCompleted"];
        }
        
        var remaining = TaskProgressDetail.TotalDueCount - TaskProgressDetail.CompletedCount;
        return string.Format(L["ProgressRemaining"], remaining);
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
