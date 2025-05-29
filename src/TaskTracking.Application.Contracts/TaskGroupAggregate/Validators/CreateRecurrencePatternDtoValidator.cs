using System;
using FluentValidation;
using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskItems;
using Microsoft.Extensions.Localization;
namespace TaskTracking.TaskGroupAggregate.Validators;

public class CreateRecurrencePatternDtoValidator : AbstractValidator<CreateRecurrencePatternDto>
{
    private readonly IStringLocalizer<TaskTrackingResource> _localizer;

    public CreateRecurrencePatternDtoValidator(IStringLocalizer<TaskTrackingResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.RecurrenceType)
            .IsInEnum()
            .WithName(_localizer["RecurrenceType"])
            .WithMessage(_localizer["The {PropertyName} field has an invalid value."]);

        RuleFor(x => x.Interval)
            .GreaterThan(0)
            .WithName(_localizer["Interval"])
            .WithMessage(_localizer[TaskTrackingDomainErrorCodes.InvalidRecurrenceInterval]);

        RuleFor(x => x.DaysOfWeek)
            .NotEmpty()
            .When(x => x.RecurrenceType == RecurrenceType.Weekly)
            .WithName(_localizer["DaysOfWeek"])
            .WithMessage(_localizer[TaskTrackingDomainErrorCodes.WeeklyRecurrenceRequiresDaysOfWeek]);

        RuleFor(x => x.DaysOfWeek)
            .Must(days => days == null || days.Count == 0)
            .When(x => x.RecurrenceType != RecurrenceType.Weekly)
            .WithName(_localizer["DaysOfWeek"])
            .WithMessage(_localizer["Days of week should only be specified for weekly recurrence."]);

        RuleFor(x => x.Occurrences)
            .GreaterThan(0)
            .When(x => x.Occurrences.HasValue)
            .WithName(_localizer["Occurrences"])
            .WithMessage(_localizer[TaskTrackingDomainErrorCodes.InvalidRecurrenceOccurrences]);

        RuleFor(x => x)
            .Must(x => x.EndDate.HasValue || x.Occurrences.HasValue)
            .WithMessage(_localizer[TaskTrackingDomainErrorCodes.RecurrenceMustHaveEndDateOrOccurrences]);
    }
}