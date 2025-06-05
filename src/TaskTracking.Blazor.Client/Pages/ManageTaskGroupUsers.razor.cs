using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using TaskTracking.Blazor.Client.Components;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.Blazor.Client.Pages;

public partial class ManageTaskGroupUsers
{
    [Parameter] public Guid Id { get; set; }

    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;

    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Inject] private IDialogService DialogService { get; set; } = null!;

    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    private TaskGroupDto? TaskGroup { get; set; }
    private List<UserTaskGroupDto> CurrentUsers { get; set; } = new();
    private List<UserSearchResultDto> SearchResults { get; set; } = new();
    private List<TaskGroupInvitationDto> Invitations { get; set; } = new();
    private string SearchKeyword { get; set; } = string.Empty;
    private bool IsLoadingUsers { get; set; } = true;
    private bool IsSearchingUsers { get; set; } = false;
    private bool IsLoadingInvitations { get; set; } = true;
    private int ActiveTabIndex { get; set; } = 0;

    private List<BreadcrumbItem> _breadcrumbItems = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadTaskGroup();
        await LoadCurrentUsers();
        await LoadInvitations();
        SetupBreadcrumbs();
    }

    private async Task LoadTaskGroup()
    {
        try
        {
            TaskGroup = await TaskGroupAppService.GetAsync(Id);
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorLoadingTaskGroup"], Severity.Error);
            Console.WriteLine($"Error loading task group: {ex.Message}");
        }
    }

    private void SetupBreadcrumbs()
    {
        _breadcrumbItems = new List<BreadcrumbItem>
        {
            new(L["TaskGroups"], "/task-groups"),
            new(TaskGroup?.Title ?? L["TaskGroup"], $"/task-groups/{Id}"),
            new(L["ManageUsers"], null, disabled: true)
        };
    }

    private async Task LoadCurrentUsers()
    {
        try
        {
            IsLoadingUsers = true;
            CurrentUsers = await TaskGroupAppService.GetTaskGroupUsersAsync(Id);
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorLoadingUsers"], Severity.Error);
            Console.WriteLine($"Error loading users: {ex.Message}");
        }
        finally
        {
            IsLoadingUsers = false;
            StateHasChanged();
        }
    }

    private async Task SearchUsers()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            SearchResults.Clear();
            StateHasChanged();
            return;
        }

        try
        {
            IsSearchingUsers = true;
            StateHasChanged();

            var input = new SearchUsersInput
            {
                Keyword = SearchKeyword,
                TaskGroupId = Id,
                MaxResultCount = 50
            };

            var result = await TaskGroupAppService.SearchUsersAsync(input);
            SearchResults = result.Items.ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorSearchingUsers"], Severity.Error);
            Console.WriteLine($"Error searching users: {ex.Message}");
        }
        finally
        {
            IsSearchingUsers = false;
            StateHasChanged();
        }
    }

    private async Task AddUser(UserSearchResultDto user)
    {
        var parameters = new DialogParameters<RoleSelectionDialog>
        {
            { x => x.UserName, user.UserName },
            { x => x.SelectedRole, UserTaskGroupRole.Subscriber }
        };

        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small };
        var result = await DialogService.ShowAsync<RoleSelectionDialog>(L["SelectRole"], parameters, options);
        var dialogResult = await result.Result;

        if (!dialogResult.Canceled && dialogResult.Data is UserTaskGroupRole selectedRole)
        {
            try
            {
                await TaskGroupAppService.AddUserAsync(Id, user.Id, selectedRole);
                Snackbar.Add(string.Format(L["UserAddedSuccessfully"], user.UserName), Severity.Success);

                // Refresh both lists
                await LoadCurrentUsers();
                await SearchUsers();
            }
            catch (Exception ex)
            {
                Snackbar.Add(L["ErrorAddingUser"], Severity.Error);
                Console.WriteLine($"Error adding user: {ex.Message}");
            }
        }
    }

    private async Task RemoveUser(UserTaskGroupDto user)
    {
        var parameters = new DialogParameters<ConfirmationDialog>
        {
            { x => x.ContentText, string.Format(L["RemoveUserConfirmation"], user.UserName) },
            { x => x.ButtonText, L["Remove"] },
            { x => x.Color, Color.Error }
        };

        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var result = await DialogService.ShowAsync<ConfirmationDialog>(L["RemoveUser"], parameters, options);
        var dialogResult = await result.Result;

        if (!dialogResult.Canceled && dialogResult.Data is true)
        {
            try
            {
                await TaskGroupAppService.RemoveUserAsync(Id, user.UserId);
                Snackbar.Add(string.Format(L["UserRemovedSuccessfully"], user.UserName), Severity.Success);

                // Refresh both lists
                await LoadCurrentUsers();
                await SearchUsers();
            }
            catch (Exception ex)
            {
                Snackbar.Add(L["ErrorRemovingUser"], Severity.Error);
                Console.WriteLine($"Error removing user: {ex.Message}");
            }
        }
    }

    private async Task ChangeUserRole(UserTaskGroupDto user)
    {
        var parameters = new DialogParameters<RoleSelectionDialog>
        {
            { x => x.UserName, user.UserName },
            { x => x.SelectedRole, user.Role },
            { x => x.IsChangingRole, true }
        };

        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small };
        var result = await DialogService.ShowAsync<RoleSelectionDialog>(L["ChangeRole"], parameters, options);
        var dialogResult = await result.Result;

        if (!dialogResult.Canceled && dialogResult.Data is UserTaskGroupRole selectedRole && selectedRole != user.Role)
        {
            await TaskGroupAppService.UpdateUserRoleAsync(Id, user.UserId, selectedRole);
            Snackbar.Add(string.Format(L["UserRoleChangedSuccessfully"], user.UserName), Severity.Success);

            // Refresh the current users list
            await LoadCurrentUsers();
        }
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

    private string GetRoleDescription(UserTaskGroupRole role)
    {
        return role switch
        {
            UserTaskGroupRole.Owner => L["OwnerRoleDescription"],
            UserTaskGroupRole.CoOwner => L["CoOwnerRoleDescription"],
            UserTaskGroupRole.Subscriber => L["SubscriberRoleDescription"],
            _ => string.Empty
        };
    }

    #region Invitation Management

    private async Task LoadInvitations()
    {
        try
        {
            IsLoadingInvitations = true;
            Invitations = await TaskGroupAppService.GetTaskGroupInvitationsAsync(Id);
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["ErrorLoadingInvitations"], Severity.Error);
            Console.WriteLine($"Error loading invitations: {ex.Message}");
        }
        finally
        {
            IsLoadingInvitations = false;
            StateHasChanged();
        }
    }

    private async Task CreateInvitation()
    {
        var parameters = new DialogParameters<InvitationCreateDialog>();
        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium };
        var result = await DialogService.ShowAsync<InvitationCreateDialog>(L["CreateInvitation"], parameters, options);
        var dialogResult = await result.Result;

        if (!dialogResult.Canceled && dialogResult.Data is CreateTaskGroupInvitationDto createDto)
        {
            try
            {
                var invitation = await TaskGroupAppService.GenerateInvitationLinkAsync(Id, createDto);
                Snackbar.Add(L["InvitationCreatedSuccessfully"], Severity.Success);

                // Refresh invitations list
                await LoadInvitations();

                // Optionally copy the link to clipboard immediately
                await CopyInvitationLink(invitation);
            }
            catch (Exception ex)
            {
                Snackbar.Add(L["ErrorCreatingInvitation"], Severity.Error);
                Console.WriteLine($"Error creating invitation: {ex.Message}");
            }
        }
    }

    private async Task CopyInvitationLink(TaskGroupInvitationDto invitation)
    {
        try
        {
            var baseUrl = NavigationManager.BaseUri.TrimEnd('/');
            var invitationUrl = $"{baseUrl}/join/{invitation.InvitationToken}";

            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", invitationUrl);
            Snackbar.Add(L["InvitationLinkCopied"], Severity.Success);
        }
        catch (Exception)
        {
            // Fallback for browsers that don't support clipboard API
            var baseUrl = NavigationManager.BaseUri.TrimEnd('/');
            var invitationUrl = $"{baseUrl}/join/{invitation.InvitationToken}";
            Snackbar.Add($"Could not copy to clipboard. Please copy manually: {invitationUrl}", Severity.Warning);
        }
    }

    private async Task DeleteInvitation(TaskGroupInvitationDto invitation)
    {
        var parameters = new DialogParameters<ConfirmationDialog>
        {
            { x => x.ContentText, L["DeleteInvitationConfirmation"] },
            { x => x.ButtonText, L["Delete"] },
            { x => x.Color, Color.Error }
        };

        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var result = await DialogService.ShowAsync<ConfirmationDialog>(L["DeleteInvitation"], parameters, options);
        var dialogResult = await result.Result;

        if (!dialogResult.Canceled && dialogResult.Data is true)
        {
            try
            {
                await TaskGroupAppService.DeleteInvitationAsync(invitation.Id);
                Snackbar.Add(L["InvitationDeletedSuccessfully"], Severity.Success);

                // Refresh invitations list
                await LoadInvitations();
            }
            catch (Exception ex)
            {
                Snackbar.Add(L["ErrorDeletingInvitation"], Severity.Error);
                Console.WriteLine($"Error deleting invitation: {ex.Message}");
            }
        }
    }

    private Color GetInvitationStatusColor(TaskGroupInvitationDto invitation)
    {
        if (invitation.IsExpired)
            return Color.Error;
        if (invitation.IsMaxUsesReached)
            return Color.Warning;
        if (invitation.IsValid)
            return Color.Success;
        return Color.Default;
    }

    private string GetInvitationStatusText(TaskGroupInvitationDto invitation)
    {
        if (invitation.IsExpired)
            return L["Expired"];
        if (invitation.IsMaxUsesReached)
            return L["MaxUsesReached"];
        if (invitation.IsValid)
            return L["Active"];
        return L["Invalid"];
    }

    #endregion
}