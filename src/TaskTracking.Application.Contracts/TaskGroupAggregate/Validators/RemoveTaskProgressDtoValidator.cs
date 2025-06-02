using FluentValidation;
using Microsoft.Extensions.Localization;
using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;

namespace TaskTracking.TaskGroupAggregate.Validators;

public class RemoveTaskProgressDtoValidator : AbstractValidator<RemoveTaskProgressDto>
{
    private readonly IStringLocalizer<TaskTrackingResource> _localizer;

    public RemoveTaskProgressDtoValidator(IStringLocalizer<TaskTrackingResource> localizer)
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
    }
}
