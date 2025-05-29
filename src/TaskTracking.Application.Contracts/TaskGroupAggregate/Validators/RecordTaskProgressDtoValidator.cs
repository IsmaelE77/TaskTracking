using System;
using System;
using FluentValidation;
using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using Microsoft.Extensions.Localization;

namespace TaskTracking.TaskGroupAggregate.Validators;

public class RecordTaskProgressDtoValidator : AbstractValidator<RecordTaskProgressDto>
{
    private readonly IStringLocalizer<TaskTrackingResource> _localizer;

    public RecordTaskProgressDtoValidator(IStringLocalizer<TaskTrackingResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.TaskItemId)
            .NotEmpty()
            .WithName(_localizer["TaskItemId"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithName(_localizer["Date"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithName(_localizer["Date"])
            .WithMessage(_localizer["Progress date cannot be in the future."]);
    }
}