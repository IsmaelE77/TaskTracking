using System;
using FluentValidation;
using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.TaskItems;
using Microsoft.Extensions.Localization;

namespace TaskTracking.TaskGroupAggregate.Validators;

public class CreateTaskItemDtoValidator : AbstractValidator<CreateTaskItemDto>
{
    private readonly IStringLocalizer<TaskTrackingResource> _localizer;

    public CreateTaskItemDtoValidator(IStringLocalizer<TaskTrackingResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithName(_localizer["Title"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.Title)
            .MaximumLength(TaskItemConsts.MaxTitleLength)
            .WithName(_localizer["Title"])
            .WithMessage(_localizer["The {PropertyName} field must not exceed {MaxLength} characters."]);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithName(_localizer["Description"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.Description)
            .MaximumLength(TaskItemConsts.MaxDescriptionLength)
            .WithName(_localizer["Description"])
            .WithMessage(_localizer["The {PropertyName} field must not exceed {MaxLength} characters."]);

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithName(_localizer["StartDate"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithName(_localizer["EndDate"])
            .WithMessage(_localizer["EndDateMustBeAfterStartDate"]);

        RuleFor(x => x.TaskType)
            .IsInEnum()
            .WithName(_localizer["TaskType"])
            .WithMessage(_localizer["The {PropertyName} field has an invalid value."]);

        RuleFor(x => x.RecurrencePattern)
            .NotNull()
            .When(x => x.TaskType == TaskType.Recurring)
            .WithName(_localizer["Recurrence"])
            .WithMessage(_localizer[TaskTrackingDomainErrorCodes.RecurrencePatternRequired]);

        RuleFor(x => x.RecurrencePattern)
            .Null()
            .When(x => x.TaskType == TaskType.OneTime)
            .WithName(_localizer["Recurrence"])
            .WithMessage(_localizer[TaskTrackingDomainErrorCodes.CannotSetRecurrencePatternForOneTimeTask]);

        RuleFor(x => x.RecurrencePattern)
            .SetValidator(new CreateRecurrencePatternDtoValidator(_localizer)!)
            .When(x => x.RecurrencePattern != null);
    }
}