using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.Blazor.Client.Pages;

public partial class ViewTaskGroup
{
    [Parameter] public Guid Id { get; set; }
    
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private ITaskItemAppService TaskItemAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private TaskGroupDto? TaskGroup { get; set; }
    private List<TaskItemDto> Tasks { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsLoadingTasks { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadTaskGroupAsync();
        await LoadTasksAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (TaskGroup?.Id != Id)
        {
            await LoadTaskGroupAsync();
            await LoadTasksAsync();
        }
    }

    private async Task LoadTaskGroupAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            TaskGroup = await TaskGroupAppService.GetAsync(Id);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
            TaskGroup = null;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            IsLoadingTasks = true;
            StateHasChanged();

            var result = await TaskItemAppService.GetTasksForGroupAsync(Id, new PagedResultRequestDto
            {
                MaxResultCount = 1000 // Load all tasks for now
            });

            Tasks = result.Items.ToList();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
            Tasks = new List<TaskItemDto>();
        }
        finally
        {
            IsLoadingTasks = false;
            StateHasChanged();
        }
    }

    private Color GetProgressColor()
    {
        if (TaskGroup == null) return Color.Primary;
        
        return TaskGroup.ProgressPercentageCompleted switch
        {
            >= 80 => Color.Success,
            >= 50 => Color.Warning,
            _ => Color.Primary
        };
    }

    private async Task OnTaskDeleted()
    {
        await LoadTasksAsync();
        await LoadTaskGroupAsync(); // Reload to update progress
        Snackbar.Add(L["TaskDeletedSuccessfully"], Severity.Success);
    }

    protected override Task HandleErrorAsync(Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Snackbar.Add(L["AnErrorOccurred"], Severity.Error);
        return Task.CompletedTask;
    }
}
