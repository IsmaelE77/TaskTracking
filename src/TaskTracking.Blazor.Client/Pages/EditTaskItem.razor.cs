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

public partial class EditTaskItem
{
    [Parameter] public Guid TaskGroupId { get; set; }
    [Parameter] public Guid TaskItemId { get; set; }
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private ITaskItemAppService ItemAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private UpdateTaskItemDtoValidator UpdateTaskItemDtoValidator { get; set; } = null!;

    private TaskItemDto? TaskItem { get; set; }
    private UpdateTaskItemDto UpdateDto { get; set; } = new();
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
        await LoadTaskGroupAndItemAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadTaskGroupAndItemAsync()
    {
        try
        {
            TaskItem = await ItemAppService.GetAsync(TaskItemId);
            
            if (TaskItem != null)
            {
                InitializeFormWithTaskData();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeFormWithTaskData()
    {
        if (TaskItem == null) return;

        // Map existing task data to update DTO
        UpdateDto = new UpdateTaskItemDto
        {
            Title = TaskItem.Title,
            Description = TaskItem.Description,
            StartDate = TaskItem.StartDate,
            EndDate = TaskItem.EndDate,
            TaskType = TaskItem.TaskType
        };

        // Set date picker values
        StartDatePicker = TaskItem.StartDate;
        EndDatePicker = TaskItem.EndDate;
        
        // Initialize recurrence pattern if task is recurring
        if (TaskItem.TaskType == TaskType.Recurring && TaskItem.RecurrencePattern != null)
        {
            var existingPattern = TaskItem.RecurrencePattern;
            
            RecurrencePattern = new CreateRecurrencePatternDto
            {
                RecurrenceType = existingPattern.RecurrenceType,
                Interval = existingPattern.Interval,
                DaysOfWeek = existingPattern.DaysOfWeek.ToList(),
                EndDate = existingPattern.EndDate,
                Occurrences = existingPattern.Occurrences
            };

            // Set end condition type
            EndConditionType = existingPattern.EndDate.HasValue ? "EndDate" : "Occurrences";
            RecurrenceEndDatePicker = existingPattern.EndDate;

            // Initialize days of week selection
            InitializeDaysOfWeekSelection(existingPattern.DaysOfWeek);
            
            UpdateDto.RecurrencePattern = RecurrencePattern;
        }
        else
        {
            // Initialize default recurrence pattern for potential conversion
            RecurrencePattern = new CreateRecurrencePatternDto
            {
                RecurrenceType = RecurrenceType.Daily,
                Interval = 1
            };
            
            // Initialize empty days of week selection
            InitializeDaysOfWeekSelection(new List<DayOfWeek>());
        }
    }

    private void InitializeDaysOfWeekSelection(IEnumerable<DayOfWeek> selectedDays)
    {
        DaysOfWeekSelection.Clear();
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            DaysOfWeekSelection[day] = selectedDays.Contains(day);
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
            UpdateDto.StartDate = StartDatePicker.Value;
        }
        UpdateDto.EndDate = EndDatePicker;

        // Handle recurrence pattern for recurring tasks
        if (UpdateDto.TaskType == TaskType.Recurring)
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

            UpdateDto.RecurrencePattern = RecurrencePattern;
        }
        else
        {
            UpdateDto.RecurrencePattern = null;
        }

        await form.Validate();

        var result = await UpdateTaskItemDtoValidator.ValidateAsync(UpdateDto);

        result.Errors.ForEach(error =>
        {
            Snackbar.Add(error.ErrorMessage, Severity.Error);
        });

        if (form.IsValid && result.IsValid)
        {
            try
            {
                IsSubmitting = true;

                await TaskGroupAppService.UpdateTaskItemAsync(TaskGroupId, TaskItemId, UpdateDto);

                Snackbar.Add(L["TaskUpdatedSuccessfully"], Severity.Success);

                // Navigate back to the task group view
                NavigationManager.NavigateTo($"/task-groups/{TaskGroupId}");
            }
            catch (Exception ex)
            {
                Snackbar.Add(L["ErrorUpdatingTask"], Severity.Error);
                Console.WriteLine($"Error updating task: {ex.Message}");
            }
            finally
            {
                IsSubmitting = false;
            }
        }
    }
}
