using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.Validators;

namespace TaskTracking.Blazor.Client.Components;

public partial class ProgressRecordingDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;
    
    [Parameter] public TaskProgressDetailDto? TaskProgressDetail { get; set; }
    [Parameter] public Guid TaskGroupId { get; set; }
    
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private RecordTaskProgressDtoValidator RecordTaskProgressDtoValidator { get; set; } = null!;
    [Inject] private RemoveTaskProgressDtoValidator RemoveTaskProgressDtoValidator { get; set; } = null!;

    private bool IsRecording { get; set; }
    private DateTime? SelectedDate { get; set; }

    private bool IsDataChanged  { get; set; } = false;
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

    private async Task OnProgressRemoved(DateOnly date)
    {
        await RemoveProgress(date);
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

            var result = await RecordTaskProgressDtoValidator.ValidateAsync(recordDto);

            result.Errors.ForEach(error =>
            {
                Snackbar.Add(error.ErrorMessage, Severity.Error);
            });

            if (result.IsValid)
            {
                await TaskGroupAppService.RecordTaskProgressAsync(TaskGroupId, recordDto);

                Snackbar.Add(L["ProgressRecordedSuccessfully"], Severity.Success);

                // Refresh the task progress detail
                TaskProgressDetail = await TaskGroupAppService.GetTaskProgressDetailAsync(
                    TaskGroupId, TaskProgressDetail.TaskItem.Id);

                IsDataChanged = true;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorRecordingProgress"], Severity.Error);
        }
        finally
        {
            IsRecording = false;
        }
    }

    private async Task RemoveProgress(DateOnly date)
    {
        if (TaskProgressDetail == null) return;

        try
        {
            IsRecording = true;

            var removeDto = new RemoveTaskProgressDto
            {
                TaskItemId = TaskProgressDetail.TaskItem.Id,
                Date = date
            };

            var result = await RemoveTaskProgressDtoValidator.ValidateAsync(removeDto);

            result.Errors.ForEach(error =>
            {
                Snackbar.Add(error.ErrorMessage, Severity.Error);
            });

            if (result.IsValid)
            {
                await TaskGroupAppService.RemoveTaskProgressAsync(TaskGroupId, removeDto);

                Snackbar.Add(L["ProgressRemovedSuccessfully"], Severity.Success);

                // Refresh the task progress detail
                TaskProgressDetail = await TaskGroupAppService.GetTaskProgressDetailAsync(
                    TaskGroupId, TaskProgressDetail.TaskItem.Id);

                IsDataChanged = true;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorRemovingProgress"], Severity.Error);
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
        if (IsDataChanged)
        {
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
        {
            MudDialog.Cancel();
        }
    }
}