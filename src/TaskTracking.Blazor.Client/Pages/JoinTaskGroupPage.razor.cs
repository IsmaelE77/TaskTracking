using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;

namespace TaskTracking.Blazor.Client.Pages;

public partial class JoinTaskGroupPage
{
    [Parameter] public string InvitationCode { get; set; } = null!;

    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private TaskGroupInvitationInfoDto? InvitationInfo { get; set; }
    private bool IsLoading { get; set; } = true;
    private bool IsJoining { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadInvitationInfoAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(InvitationCode))
        {
            await LoadInvitationInfoAsync();
        }
    }

    private async Task LoadInvitationInfoAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            InvitationInfo = await TaskGroupAppService.GetInvitationInfoAsync(InvitationCode);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
            InvitationInfo = null;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task JoinTaskGroup()
    {
        if (!CurrentUser.IsAuthenticated)
        {
            NavigationManager.NavigateTo($"/authentication/login?returnUrl={Uri.EscapeDataString(NavigationManager.Uri)}");
            return;
        }

        try
        {
            IsJoining = true;
            StateHasChanged();

            var input = new JoinTaskGroupByInvitationDto
            {
                InvitationCode = InvitationCode
            };

            await TaskGroupAppService.JoinByInvitationAsync(input);

            Snackbar.Add(L["JoinedTaskGroupSuccessfully"], Severity.Success);
            
            // Redirect to the task group page or my task groups
            NavigationManager.NavigateTo("/task-groups/my");
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
        finally
        {
            IsJoining = false;
            StateHasChanged();
        }
    }

    protected override Task HandleErrorAsync(Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Snackbar.Add(L["AnErrorOccurred"], Severity.Error);
        return Task.CompletedTask;
    }
}
