using System;
using FluentValidation;
using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using Microsoft.Extensions.Localization;

namespace TaskTracking.TaskGroupAggregate.Validators;

public class UpdateTaskGroupDtoValidator : AbstractValidator<UpdateTaskGroupDto>
{
    private readonly IStringLocalizer<TaskTrackingResource> _localizer;

    public UpdateTaskGroupDtoValidator(IStringLocalizer<TaskTrackingResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithName(_localizer["Title"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.Title)
            .MaximumLength(TaskGroupConsts.MaxTitleLength)
            .WithName(_localizer["Title"])
            .WithMessage(_localizer["The {PropertyName} field must not exceed {MaxLength} characters."]);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithName(_localizer["Description"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.Description)
            .MaximumLength(TaskGroupConsts.MaxDescriptionLength)
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
    }
}
