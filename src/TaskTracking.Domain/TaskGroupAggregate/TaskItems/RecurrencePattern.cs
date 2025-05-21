using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TaskTracking.TaskGroupAggregate.TaskItems;

public class RecurrencePattern : ValueObject
{
    public RecurrenceType RecurrenceType { get; private set; }
    public int Interval { get; private set; }
    public List<DayOfWeek>? DaysOfWeek { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int? Occurrences { get; private set; }

    private RecurrencePattern()
    {
        // Required by serialization
    }

    internal static RecurrencePattern CreateDaily(int interval, DateTime? endDate = null, int? occurrences = null)
    {
        return new RecurrencePattern(RecurrenceType.Daily, interval, null, endDate, occurrences);
    }

    internal static RecurrencePattern CreateWeekly(int interval, List<DayOfWeek> daysOfWeek, DateTime? endDate = null,
        int? occurrences = null)
    {
        return new RecurrencePattern(RecurrenceType.Weekly, interval, daysOfWeek, endDate, occurrences);
    }

    internal static RecurrencePattern CreateMonthly(int interval, DateTime? endDate = null, int? occurrences = null)
    {
        return new RecurrencePattern(RecurrenceType.Monthly, interval, null, endDate, occurrences);
    }

    private RecurrencePattern(
        RecurrenceType recurrenceType,
        int interval,
        List<DayOfWeek> daysOfWeek,
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

        RecurrenceType = recurrenceType;
        Interval = interval;
        DaysOfWeek = daysOfWeek;
        EndDate = endDate;
        Occurrences = occurrences;
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