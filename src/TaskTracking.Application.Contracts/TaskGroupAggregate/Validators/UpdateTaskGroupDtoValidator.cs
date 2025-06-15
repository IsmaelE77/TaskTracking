using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using Microsoft.Extensions.Localization;
using Volo.Abp.Timing;

namespace TaskTracking.TaskGroupAggregate.Validators;

public class UpdateTaskGroupDtoValidator : AbstractValidator<UpdateTaskGroupDto>
{
    private readonly IStringLocalizer<TaskTrackingResource> _localizer;
    private readonly IClock _clock;

    public UpdateTaskGroupDtoValidator(IStringLocalizer<TaskTrackingResource> localizer, IClock clock)
    {
        _localizer = localizer;
        _clock = clock;

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
            .GreaterThanOrEqualTo(_clock.Now.Date)
            .WithName(_localizer["StartDate"])
            .WithMessage(_localizer["StartDateCannotBeInPast"]);

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


    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UpdateTaskGroupDto>.CreateWithOptions((UpdateTaskGroupDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
        {
            return [];
        }

        return result.Errors.Select(e => e.ErrorMessage);
    };
}