using System;
using System;
using FluentValidation;
using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using Microsoft.Extensions.Localization;
using Volo.Abp.Timing;

namespace TaskTracking.TaskGroupAggregate.Validators;

public class RecordTaskProgressDtoValidator : AbstractValidator<RecordTaskProgressDto>
{
    private readonly IStringLocalizer<TaskTrackingResource> _localizer;
    private IClock _clock;

    public RecordTaskProgressDtoValidator(IStringLocalizer<TaskTrackingResource> localizer, IClock clock)
    {
        _localizer = localizer;
        _clock = clock;

        RuleFor(x => x.TaskItemId)
            .NotEmpty()
            .WithName(_localizer["TaskItemId"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithName(_localizer["Date"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(_clock.Now.Date))
            .WithName(_localizer["Date"])
            .WithMessage(_localizer[TaskTrackingDomainErrorCodes.ProgressDateInFuture]);
    }
}