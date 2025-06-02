using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TaskTracking.Blazor.Client.Components;

public partial class ConfirmationDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public string ContentText { get; set; } = string.Empty;

    [Parameter]
    public string ButtonText { get; set; } = string.Empty;

    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    private void Submit() => MudDialog.Close(DialogResult.Ok(true));

    private void Cancel() => MudDialog.Cancel();
}
