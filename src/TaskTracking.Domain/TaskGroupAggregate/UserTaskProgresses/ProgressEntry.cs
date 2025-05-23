using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace TaskTracking.TaskGroupAggregate.UserTaskProgresses;

public class ProgressEntry : ValueObject
{
    public DateOnly Date { get; private set; }

    public ProgressEntry(DateOnly date)
    {
        Date = date;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Date;
    }
}