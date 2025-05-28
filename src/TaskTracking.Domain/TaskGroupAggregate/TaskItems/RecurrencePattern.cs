using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TaskTracking.TaskGroupAggregate.TaskItems;

public class RecurrencePattern : ValueObject
{
    public RecurrenceType RecurrenceType { get; private set; }
    public int Interval { get; private set; }

    // Bitmask for days of week (replaces DaysOfWeek collection)
    // Sunday = 1, Monday = 2, Tuesday = 4, Wednesday = 8, Thursday = 16, Friday = 32, Saturday = 64
    public int DaysOfWeekFlags { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int? Occurrences { get; private set; }

    [NotMapped] public IReadOnlyCollection<DayOfWeek> DaysOfWeek => GetDaysOfWeekFromFlags();

    private RecurrencePattern()
    {
        // Required by serialization
    }

    public static RecurrencePattern CreateDaily(int interval, DateTime? endDate = null, int? occurrences = null)
    {
        return new RecurrencePattern(RecurrenceType.Daily, interval, null, endDate, occurrences);
    }

    public static RecurrencePattern CreateWeekly(int interval, List<DayOfWeek> daysOfWeek, DateTime? endDate = null,
        int? occurrences = null)
    {
        return new RecurrencePattern(RecurrenceType.Weekly, interval, daysOfWeek, endDate, occurrences);
    }

    public static RecurrencePattern CreateMonthly(int interval, DateTime? endDate = null, int? occurrences = null)
    {
        return new RecurrencePattern(RecurrenceType.Monthly, interval, null, endDate, occurrences);
    }

    private RecurrencePattern(
        RecurrenceType recurrenceType,
        int interval,
        List<DayOfWeek>? daysOfWeek,
        DateTime? endDate,
        int? occurrences)
    {
        if (interval <= 0)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvalidRecurrenceInterval);
        }

        if (occurrences.HasValue && occurrences.Value <= 0)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvalidRecurrenceOccurrences);
        }

        if (recurrenceType == RecurrenceType.Weekly && (daysOfWeek == null || daysOfWeek.Count == 0))
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.WeeklyRecurrenceRequiresDaysOfWeek);
        }

        if (endDate == null && occurrences == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.RecurrenceMustHaveEndDateOrOccurrences);
        }

        RecurrenceType = recurrenceType;
        Interval = interval;
        EndDate = endDate;
        Occurrences = occurrences;

        if (daysOfWeek != null)
        {
            SetDaysOfWeek(daysOfWeek);
        }
    }


    public void SetDaysOfWeek(IEnumerable<DayOfWeek> daysOfWeek)
    {
        DaysOfWeekFlags = 0;

        foreach (var day in daysOfWeek)
        {
            AddDay(day);
        }
    }

    public void AddDay(DayOfWeek day)
    {
        DaysOfWeekFlags |= (1 << (int)day);
    }

    public void RemoveDay(DayOfWeek day)
    {
        DaysOfWeekFlags &= ~(1 << (int)day);
    }

    public bool HasDay(DayOfWeek day)
    {
        return (DaysOfWeekFlags & (1 << (int)day)) != 0;
    }


    private IReadOnlyCollection<DayOfWeek> GetDaysOfWeekFromFlags()
    {
        var days = new List<DayOfWeek>();

        for (int i = 0; i < 7; i++)
        {
            if ((DaysOfWeekFlags & (1 << i)) != 0)
            {
                days.Add((DayOfWeek)i);
            }
        }

        return days.AsReadOnly();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return RecurrenceType;
        yield return Interval;
        yield return DaysOfWeek;
        yield return EndDate;
        yield return Occurrences;
    }
}