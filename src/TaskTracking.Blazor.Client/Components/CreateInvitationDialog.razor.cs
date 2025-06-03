using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;

namespace TaskTracking.Blazor.Client.Components;

public partial class CreateInvitationDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public Guid TaskGroupId { get; set; }

    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private bool IsCreating { get; set; }
    private bool IsFormValid { get; set; }
    private int? ExpirationHours { get; set; }

    private MudForm Form { get; set; } = null!;

    private async Task CreateInvitation()
    {
        try
        {
            IsCreating = true;
            StateHasChanged();

            var input = new CreateTaskGroupInvitationDto
            {
                ExpirationHours = ExpirationHours
            };

            var invitation = await TaskGroupAppService.GenerateInvitationAsync(TaskGroupId, input);

            Snackbar.Add(L["InvitationCreatedSuccessfully"], Severity.Success);

            // Automatically copy the invitation link
            await CopyInvitationLink(invitation);

            // Close dialog with the created invitation as result
            MudDialog.Close(DialogResult.Ok(invitation));
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
        finally
        {
            IsCreating = false;
            StateHasChanged();
        }
    }

    private async Task CopyInvitationLink(TaskGroupInvitationDto invitation)
    {
        try
        {
            var baseUrl = NavigationManager.BaseUri.TrimEnd('/');
            var invitationUrl = $"{baseUrl}/join/{invitation.InvitationCode}";
            
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", invitationUrl);
            Snackbar.Add(L["InvitationLinkCopied"], Severity.Success);
        }
        catch (Exception)
        {
            // Fallback for browsers that don't support clipboard API
            Snackbar.Add("Could not copy to clipboard. Please copy manually: " + invitation.InvitationCode, Severity.Warning);
        }
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    protected override Task HandleErrorAsync(Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Snackbar.Add(L["AnErrorOccurred"], Severity.Error);
        return Task.CompletedTask;
    }
}
