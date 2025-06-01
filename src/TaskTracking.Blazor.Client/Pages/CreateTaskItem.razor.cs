using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.Validators;
using Severity = MudBlazor.Severity;

namespace TaskTracking.Blazor.Client.Pages;

public partial class CreateTaskItem
{
    [Parameter] public Guid TaskGroupId { get; set; }
    
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private CreateTaskItemDtoValidator CreateTaskItemDtoValidator { get; set; } = null!;

    private TaskGroupDto? TaskGroup { get; set; }
    private CreateTaskItemDto CreateDto { get; set; } = new();
    private CreateRecurrencePatternDto RecurrencePattern { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsSubmitting { get; set; } = false;
    
    // Date picker properties
    private DateTime? StartDatePicker { get; set; }
    private DateTime? EndDatePicker { get; set; }
    private DateTime? RecurrenceEndDatePicker { get; set; }
    
    // Recurrence pattern properties
    private string EndConditionType { get; set; } = "EndDate";
    private Dictionary<DayOfWeek, bool> DaysOfWeekSelection { get; set; } = new();

    private MudForm form;

    protected override async Task OnInitializedAsync()
    {
        await LoadTaskGroupAsync();
        InitializeForm();
        await base.OnInitializedAsync();
    }

    private async Task LoadTaskGroupAsync()
    {
        try
        {
            TaskGroup = await TaskGroupAppService.GetAsync(TaskGroupId);
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

    private void InitializeForm()
    {
        // Initialize with default values
        StartDatePicker = DateTime.Today;
        CreateDto.StartDate = DateTime.Today;
        CreateDto.TaskType = TaskType.OneTime;
        
        // Initialize recurrence pattern
        RecurrencePattern.RecurrenceType = RecurrenceType.Daily;
        RecurrencePattern.Interval = 1;
        
        // Initialize days of week selection
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            DaysOfWeekSelection[day] = false;
        }
    }

    private string GetIntervalHelperText()
    {
        return RecurrencePattern.RecurrenceType switch
        {
            RecurrenceType.Daily => L["EveryNDays"],
            RecurrenceType.Weekly => L["EveryNWeeks"],
            RecurrenceType.Monthly => L["EveryNMonths"],
            _ => L["Interval"]
        };
    }

    private bool GetDayOfWeekSelection(DayOfWeek day)
    {
        return DaysOfWeekSelection.GetValueOrDefault(day, false);
    }

    private void SetDayOfWeekSelection(DayOfWeek day, bool value)
    {
        DaysOfWeekSelection[day] = value;
    }

    private async Task Submit()
    {
        // Update dates from pickers
        if (StartDatePicker.HasValue)
        {
            CreateDto.StartDate = StartDatePicker.Value;
        }
        CreateDto.EndDate = EndDatePicker;

        // Handle recurrence pattern for recurring tasks
        if (CreateDto.TaskType == TaskType.Recurring)
        {
            // Set days of week for weekly recurrence
            if (RecurrencePattern.RecurrenceType == RecurrenceType.Weekly)
            {
                RecurrencePattern.DaysOfWeek = DaysOfWeekSelection
                    .Where(kvp => kvp.Value)
                    .Select(kvp => kvp.Key)
                    .ToList();
            }
            else
            {
                RecurrencePattern.DaysOfWeek.Clear();
            }

            // Set end condition
            if (EndConditionType == "EndDate")
            {
                RecurrencePattern.EndDate = RecurrenceEndDatePicker;
                RecurrencePattern.Occurrences = null;
            }
            else
            {
                RecurrencePattern.EndDate = null;
                // Occurrences is already bound to the numeric field
            }

            CreateDto.RecurrencePattern = RecurrencePattern;
        }
        else
        {
            CreateDto.RecurrencePattern = null;
        }

        await form.Validate();

        var result = await CreateTaskItemDtoValidator.ValidateAsync(CreateDto);

        result.Errors.ForEach(error =>
        {
            Snackbar.Add(error.ErrorMessage, Severity.Error);
        });

        if (form.IsValid && result.IsValid)
        {
            try
            {
                IsSubmitting = true;

                await TaskGroupAppService.CreateTaskItemAsync(TaskGroupId, CreateDto);

                Snackbar.Add(L["TaskCreatedSuccessfully"], Severity.Success);

                // Navigate back to the task group view
                NavigationManager.NavigateTo($"/task-groups/{TaskGroupId}");
            }
            catch (Exception ex)
            {
                Snackbar.Add(L["ErrorCreatingTask"], Severity.Error);
                Console.WriteLine($"Error creating task: {ex.Message}");
            }
            finally
            {
                IsSubmitting = false;
            }
        }
    }
}
