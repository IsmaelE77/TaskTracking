using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Localization;
using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;

namespace TaskTracking.TaskGroupAggregate.Validators;

public class CreateTaskGroupInvitationDtoValidator : AbstractValidator<CreateTaskGroupInvitationDto>
{
    private readonly IStringLocalizer<TaskTrackingResource> _localizer;

    public CreateTaskGroupInvitationDtoValidator(IStringLocalizer<TaskTrackingResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.ExpirationHours)
            .GreaterThan(0)
            .WithName(_localizer["ExpirationHours"])
            .WithMessage(_localizer["The {PropertyName} field must be greater than 0."]);

        RuleFor(x => x.ExpirationHours)
            .LessThanOrEqualTo(TaskGroupInvitationConsts.MaxExpirationHours)
            .WithName(_localizer["ExpirationHours"])
            .WithMessage(_localizer["The {PropertyName} field must not exceed {ComparisonValue} hours."]);

        RuleFor(x => x.MaxUses)
            .GreaterThanOrEqualTo(0)
            .WithName(_localizer["MaxUses"])
            .WithMessage(_localizer["The {PropertyName} field must be 0 or greater."]);

        RuleFor(x => x.MaxUses)
            .LessThanOrEqualTo(TaskGroupInvitationConsts.MaxAllowedUses)
            .WithName(_localizer["MaxUses"])
            .WithMessage(_localizer["The {PropertyName} field must not exceed {ComparisonValue}."]);

        RuleFor(x => x.DefaultRole)
            .IsInEnum()
            .WithName(_localizer["DefaultRole"])
            .WithMessage(_localizer["The {PropertyName} field has an invalid value."]);

        RuleFor(x => x.DefaultRole)
            .NotEqual(UserTaskGroupRole.Owner)
            .WithName(_localizer["DefaultRole"])
            .WithMessage(_localizer["Cannot set default role to Owner through invitations."]);
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CreateTaskGroupInvitationDto>.CreateWithOptions((CreateTaskGroupInvitationDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
        {
            return [];
        }

        return result.Errors.Select(e => e.ErrorMessage);
    };
}
