using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

namespace TaskTracking.Blazor.Client.Components;

public partial class ProgressCalendar
{
    [Parameter] public TaskProgressDetailDto? TaskProgressDetail { get; set; }
    [Parameter] public EventCallback<DateOnly> OnDateSelected { get; set; }
    [Parameter] public bool IsRecording { get; set; }

    private DateTime CurrentMonth { get; set; } = DateTime.Today;

    protected override void OnInitialized()
    {
        // Start with the current month or the task start month
        if (TaskProgressDetail?.TaskItem.StartDate != null)
        {
            CurrentMonth = new DateTime(TaskProgressDetail.TaskItem.StartDate.Year, 
                                      TaskProgressDetail.TaskItem.StartDate.Month, 1);
        }
        else
        {
            CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }
    }

    private void PreviousMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
    }

    private void NextMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
    }

    private string[] GetDayNames()
    {
        var culture = CultureInfo.CurrentCulture;
        var dayNames = new string[7];
        
        // Start with Sunday (0) or Monday (1) based on culture
        var firstDayOfWeek = (int)culture.DateTimeFormat.FirstDayOfWeek;
        
        for (int i = 0; i < 7; i++)
        {
            var dayIndex = (firstDayOfWeek + i) % 7;
            dayNames[i] = culture.DateTimeFormat.AbbreviatedDayNames[dayIndex];
        }
        
        return dayNames;
    }

    private List<List<DateTime>> GetCalendarWeeks()
    {
        var weeks = new List<List<DateTime>>();
        var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        
        var culture = CultureInfo.CurrentCulture;
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        
        // Find the first day to display (might be from previous month)
        var startDate = firstDayOfMonth;
        while (startDate.DayOfWeek != firstDayOfWeek)
        {
            startDate = startDate.AddDays(-1);
        }
        
        // Generate weeks
        var currentDate = startDate;
        while (currentDate <= lastDayOfMonth || weeks.Count == 0 || weeks.Last().Count < 7)
        {
            if (weeks.Count == 0 || weeks.Last().Count == 7)
            {
                weeks.Add(new List<DateTime>());
            }
            
            weeks.Last().Add(currentDate);
            currentDate = currentDate.AddDays(1);
            
            // Stop after 6 weeks to prevent infinite loop
            if (weeks.Count >= 6) break;
        }
        
        return weeks;
    }

    private string GetDayClass(DateTime day)
    {
        var classes = new List<string> { "calendar-day" };
        
        if (day.Month != CurrentMonth.Month)
        {
            classes.Add("other-month");
        }
        
        if (day.Date == DateTime.Today)
        {
            classes.Add("today");
        }
        
        var dayStatus = GetDayStatus(day);
        if (dayStatus == DayStatus.Due && !IsRecording)
        {
            classes.Add("clickable");
        }
        
        return string.Join(" ", classes);
    }

    private string GetDayTextClass(DateTime day)
    {
        if (day.Month != CurrentMonth.Month)
        {
            return "text-muted";
        }
        
        return "";
    }

    private DayStatus GetDayStatus(DateTime day)
    {
        if (TaskProgressDetail == null) return DayStatus.NotDue;
        
        var dateOnly = DateOnly.FromDateTime(day);
        
        // Check if this date is completed
        if (TaskProgressDetail.CompletedDates.Contains(dateOnly))
        {
            return DayStatus.Completed;
        }
        
        // Check if this date is due
        if (TaskProgressDetail.DueDates.Contains(dateOnly))
        {
            if (day.Date < DateTime.Today)
            {
                return DayStatus.Overdue;
            }
            else
            {
                return DayStatus.Due;
            }
        }
        
        return DayStatus.NotDue;
    }

    private async Task OnDayClick(DateTime day)
    {
        if (IsRecording) return;
        
        var dayStatus = GetDayStatus(day);
        if (dayStatus == DayStatus.Due)
        {
            var dateOnly = DateOnly.FromDateTime(day);
            await OnDateSelected.InvokeAsync(dateOnly);
        }
    }

    private enum DayStatus
    {
        NotDue,
        Due,
        Completed,
        Overdue
    }
}
