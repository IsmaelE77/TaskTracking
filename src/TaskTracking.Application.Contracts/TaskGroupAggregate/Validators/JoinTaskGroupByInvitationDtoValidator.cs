using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Localization;
using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.TaskGroupInvitations;

namespace TaskTracking.TaskGroupAggregate.Validators;

public class JoinTaskGroupByInvitationDtoValidator : AbstractValidator<JoinTaskGroupByInvitationDto>
{
    private readonly IStringLocalizer<TaskTrackingResource> _localizer;

    public JoinTaskGroupByInvitationDtoValidator(IStringLocalizer<TaskTrackingResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.InvitationToken)
            .NotEmpty()
            .WithName(_localizer["InvitationToken"])
            .WithMessage(_localizer["The {PropertyName} field is required."]);

        RuleFor(x => x.InvitationToken)
            .Length(TaskGroupInvitationConsts.TokenLength)
            .WithName(_localizer["InvitationToken"])
            .WithMessage(_localizer["The {PropertyName} field must be exactly {ExpectedLength} characters long."]);

        RuleFor(x => x.InvitationToken)
            .Matches("^[A-Za-z0-9_-]+$")
            .WithName(_localizer["InvitationToken"])
            .WithMessage(_localizer["The {PropertyName} field contains invalid characters."]);
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<JoinTaskGroupByInvitationDto>.CreateWithOptions((JoinTaskGroupByInvitationDto)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
        {
            return [];
        }

        return result.Errors.Select(e => e.ErrorMessage);
    };
}
