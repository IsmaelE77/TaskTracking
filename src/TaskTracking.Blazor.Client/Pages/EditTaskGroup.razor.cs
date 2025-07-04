using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Validators;

namespace TaskTracking.Blazor.Client.Pages;

public partial class EditTaskGroup
{
    [Parameter] public Guid Id { get; set; }
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private UpdateTaskGroupDtoValidator UpdateTaskGroupDtoValidator { get; set; } = null!;

    private TaskGroupDto? TaskGroup { get; set; }
    private UpdateTaskGroupDto UpdateDto { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsSubmitting { get; set; } = false;
    
    // Date picker properties
    private DateTime? StartDatePicker { get; set; }
    private DateTime? EndDatePicker { get; set; }
    
    private MudForm form;

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
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleValidSubmit()
    {
        // Update DTO with date picker values
        if (StartDatePicker.HasValue)
        {
            UpdateDto.StartDate = StartDatePicker.Value;
        }
        UpdateDto.EndDate = EndDatePicker;

        await form.Validate();
        
        var result = await UpdateTaskGroupDtoValidator.ValidateAsync(UpdateDto);
        
        result.Errors.ForEach(error =>
        {
            Snackbar.Add(error.ErrorMessage, Severity.Error);
        });

        if (form.IsValid && result.IsValid)
        {
            try
            {
                IsSubmitting = true;
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
}