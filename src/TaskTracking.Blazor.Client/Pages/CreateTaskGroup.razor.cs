using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;

namespace TaskTracking.Blazor.Client.Pages;

public partial class CreateTaskGroup
{
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private CreateTaskGroupDto CreateDto { get; set; } = new();
    private bool IsSubmitting { get; set; } = false;
    
    // Date picker properties
    private DateTime? StartDatePicker { get; set; }
    private DateTime? EndDatePicker { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Initialize with default values
        StartDatePicker = DateTime.Today;
        CreateDto.StartDate = DateTime.Today;
        
        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            IsSubmitting = true;

            // Update DTO with date picker values
            if (StartDatePicker.HasValue)
            {
                CreateDto.StartDate = StartDatePicker.Value;
            }
            CreateDto.EndDate = EndDatePicker;

            // Validate dates
            if (CreateDto.EndDate.HasValue && CreateDto.EndDate.Value < CreateDto.StartDate)
            {
                Snackbar.Add(L["EndDateMustBeAfterStartDate"], Severity.Warning);
                return;
            }

            // Validate start date is not in the past
            if (CreateDto.StartDate.Date < DateTime.Today)
            {
                Snackbar.Add(L["StartDateCannotBeInPast"], Severity.Warning);
                return;
            }

            var result = await TaskGroupAppService.CreateAsync(CreateDto);
            
            Snackbar.Add(L["TaskGroupCreatedSuccessfully"], Severity.Success);
            
            // Navigate to the newly created task group or back to dashboard
            NavigationManager.NavigateTo("/");
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorCreatingTaskGroup"], Severity.Error);
            Console.WriteLine($"Error creating task group: {ex.Message}");
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private void ResetForm()
    {
        CreateDto = new CreateTaskGroupDto();
        StartDatePicker = DateTime.Today;
        EndDatePicker = null;
        CreateDto.StartDate = DateTime.Today;
    }
}
