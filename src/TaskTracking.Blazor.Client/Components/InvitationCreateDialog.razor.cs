using System;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;

namespace TaskTracking.Blazor.Client.Components;

public partial class InvitationCreateDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public bool IsCreating { get; set; } = false;

    private MudForm Form { get; set; } = null!;
    private bool IsFormValid { get; set; } = false;
    private string[] Errors { get; set; } = Array.Empty<string>();

    private CreateTaskGroupInvitationDto Model { get; set; } = new()
    {
        ExpirationHours = TaskGroupInvitationConsts.DefaultExpirationHours,
        MaxUses = TaskGroupInvitationConsts.DefaultMaxUses,
        DefaultRole = UserTaskGroupRole.Subscriber
    };

    private void Submit() => MudDialog.Close(DialogResult.Ok(Model));

    private void Cancel() => MudDialog.Cancel();

    private string GetExpirationPreview()
    {
        var expirationDate = DateTime.Now.AddHours(Model.ExpirationHours);
        return expirationDate.ToString("MMM dd, yyyy HH:mm");
    }

    private string GetMaxUsagePreview()
    {
        return Model.MaxUses == 0 ? L["Unlimited"] : Model.MaxUses.ToString();
    }
}
