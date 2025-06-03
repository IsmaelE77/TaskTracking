using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;

namespace TaskTracking.Blazor.Client.Components;

public partial class RoleSelectionDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public string UserName { get; set; } = string.Empty;

    [Parameter]
    public UserTaskGroupRole SelectedRole { get; set; } = UserTaskGroupRole.Subscriber;

    [Parameter]
    public bool IsChangingRole { get; set; } = false;

    private void Submit() => MudDialog.Close(DialogResult.Ok(SelectedRole));

    private void Cancel() => MudDialog.Cancel();
}
