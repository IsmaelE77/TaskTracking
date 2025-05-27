using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;

namespace TaskTracking.Blazor.Client.Pages;

public partial class EditTaskGroup
{
    [Parameter] public Guid Id { get; set; }
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private TaskGroupDto? TaskGroup { get; set; }
    private UpdateTaskGroupDto UpdateDto { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsSubmitting { get; set; } = false;
    
    // Date picker properties
    private DateTime? StartDatePicker { get; set; }
    private DateTime? EndDatePicker { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadTaskGroup();
    }

    private async Task LoadTaskGroup()
    {
        try
        {
            IsLoading = true;
            TaskGroup = await TaskGroupAppService.GetAsync(Id);
            
            if (TaskGroup != null)
            {
                // Map the existing data to the update DTO
                UpdateDto = new UpdateTaskGroupDto
                {
                    Title = TaskGroup.Title,
                    Description = TaskGroup.Description,
                    StartDate = TaskGroup.StartDate,
                    EndDate = TaskGroup.EndDate
                };
                
                // Set date picker values
                StartDatePicker = TaskGroup.StartDate;
                EndDatePicker = TaskGroup.EndDate;

            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorLoadingTaskGroup"], Severity.Error);
            Console.WriteLine($"Error loading task group: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            IsSubmitting = true;

            // Update DTO with date picker values
            if (StartDatePicker.HasValue)
            {
                UpdateDto.StartDate = StartDatePicker.Value;
            }
            UpdateDto.EndDate = EndDatePicker;

            // Validate dates
            if (UpdateDto.EndDate.HasValue && UpdateDto.EndDate.Value < UpdateDto.StartDate)
            {
                Snackbar.Add(L["EndDateMustBeAfterStartDate"], Severity.Warning);
                return;
            }

            await TaskGroupAppService.UpdateAsync(Id, UpdateDto);
            
            Snackbar.Add(L["TaskGroupUpdatedSuccessfully"], Severity.Success);
            NavigationManager.NavigateTo("/");
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorUpdatingTaskGroup"], Severity.Error);
            Console.WriteLine($"Error updating task group: {ex.Message}");
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}