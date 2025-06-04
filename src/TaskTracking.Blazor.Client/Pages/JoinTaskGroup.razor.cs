using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;

namespace TaskTracking.Blazor.Client.Pages;

public partial class JoinTaskGroup
{
    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public string InvitationCode { get; set; } = string.Empty;

    private TaskGroupInvitationDetailsDto? InvitationDetails { get; set; }
    private bool IsLoading { get; set; } = true;
    private bool IsJoining { get; set; } = false;
    private bool HasError { get; set; } = false;
    private string ErrorMessage { get; set; } = string.Empty;
    private bool IsAuthenticated { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        // Check authentication status
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        IsAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;

        await LoadInvitationDetails();
    }

    private async Task LoadInvitationDetails()
    {
        try
        {
            IsLoading = true;
            HasError = false;

            if (string.IsNullOrWhiteSpace(InvitationCode))
            {
                HasError = true;
                ErrorMessage = L["InvalidInvitationToken"];
                return;
            }

            InvitationDetails = await TaskGroupAppService.GetInvitationDetailsAsync(InvitationCode);
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message.Contains("InvitationNotFound") 
                ? L["InvitationNotFound"] 
                : L["ErrorLoadingInvitation"];
            Console.WriteLine($"Error loading invitation details: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task JoinGroup()
    {
        if (!IsAuthenticated || InvitationDetails == null || !InvitationDetails.IsValid)
            return;

        try
        {
            IsJoining = true;
            StateHasChanged();

            var joinDto = new JoinTaskGroupByInvitationDto
            {
                InvitationToken = InvitationCode
            };

            var result = await TaskGroupAppService.JoinTaskGroupByInvitationAsync(joinDto);
            
            Snackbar.Add(string.Format(L["SuccessfullyJoinedGroup"], InvitationDetails.TaskGroupTitle), Severity.Success);
            
            // Navigate to the task group
            NavigationManager.NavigateTo($"/task-groups/{InvitationDetails.TaskGroupId}");
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message switch
            {
                var msg when msg.Contains("UserAlreadyInGroup") => L["AlreadyMemberOfGroup"],
                var msg when msg.Contains("InvitationExpired") => L["InvitationExpiredMessage"],
                var msg when msg.Contains("InvitationMaxUsesReached") => L["InvitationMaxUsesReachedMessage"],
                _ => L["ErrorJoiningGroup"]
            };
            
            Snackbar.Add(errorMessage, Severity.Error);
            Console.WriteLine($"Error joining group: {ex.Message}");
        }
        finally
        {
            IsJoining = false;
            StateHasChanged();
        }
    }

    private void Login()
    {
        // Store the current URL to return after login
        var returnUrl = NavigationManager.Uri;
        NavigationManager.NavigateTo($"/authentication/login?returnUrl={Uri.EscapeDataString(returnUrl)}", forceLoad: true);
    }

    private Color GetRoleColor(UserTaskGroupRole role)
    {
        return role switch
        {
            UserTaskGroupRole.Owner => Color.Warning,
            UserTaskGroupRole.CoOwner => Color.Info,
            UserTaskGroupRole.Subscriber => Color.Success,
            _ => Color.Default
        };
    }
}
