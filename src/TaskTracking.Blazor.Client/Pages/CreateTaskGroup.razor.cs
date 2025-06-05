using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Validators;
using Severity = MudBlazor.Severity;

namespace TaskTracking.Blazor.Client.Pages;

public partial class CreateTaskGroup
{
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private CreateTaskGroupDtoValidator CreateTaskGroupDtoValidator { get; set; } = null!;

    private CreateTaskGroupDto CreateDto { get; set; } = new();
    private bool IsSubmitting { get; set; } = false;
    
    // Date picker properties
    private DateTime? StartDatePicker { get; set; }
    private DateTime? EndDatePicker { get; set; }

    private MudForm form;

    protected override async Task OnInitializedAsync()
    {
        // Initialize with default values
        StartDatePicker = DateTime.Today;
        CreateDto.StartDate = DateTime.Today;
        
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        if (StartDatePicker.HasValue)
        {
            CreateDto.StartDate = StartDatePicker.Value;
        }
        CreateDto.EndDate = EndDatePicker;

        await form.Validate();

        var result = await CreateTaskGroupDtoValidator.ValidateAsync(CreateDto);

        result.Errors.ForEach(error =>
        {
            Snackbar.Add(error.ErrorMessage, Severity.Error);
        });

        if (form.IsValid && result.IsValid)
        {
            try
            {
                IsSubmitting = true;

                await TaskGroupAppService.CreateAsync(CreateDto);

                Snackbar.Add(L["TaskGroupCreatedSuccessfully"], Severity.Success);

                // Navigate to the newly created task group or back to dashboard
                NavigationManager.NavigateTo("/");
            }
            finally
            {
                IsSubmitting = false;
            }
        }
    }
}