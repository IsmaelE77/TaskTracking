using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using TaskTracking.TaskGroupAggregate;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroupInvitations;
using Volo.Abp.Application.Dtos;

namespace TaskTracking.Blazor.Client.Components;

public partial class TaskGroupInvitationManagement
{
    [Parameter] public Guid TaskGroupId { get; set; }
    [Parameter] public EventCallback OnInvitationCreated { get; set; }

    [Inject] private ITaskGroupAppService TaskGroupAppService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private List<TaskGroupInvitationDto> Invitations { get; set; } = new();
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadInvitationsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (TaskGroupId != Guid.Empty)
        {
            await LoadInvitationsAsync();
        }
    }

    private async Task LoadInvitationsAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            var result = await TaskGroupAppService.GetInvitationsAsync(TaskGroupId, new PagedResultRequestDto
            {
                MaxResultCount = 100
            });

            Invitations = result.Items.ToList();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
            Invitations = new List<TaskGroupInvitationDto>();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task OpenCreateInvitationDialog()
    {
        var parameters = new DialogParameters<CreateInvitationDialog>
        {
            { x => x.TaskGroupId, TaskGroupId }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseButton = true
        };

        var result = await DialogService.ShowAsync<CreateInvitationDialog>(L["CreateInvitationLink"], parameters, options);
        var dialogResult = await result.Result;

        if (!dialogResult.Canceled && dialogResult.Data is TaskGroupInvitationDto)
        {
            await LoadInvitationsAsync();
            // No need to notify parent - we handle our own data refresh
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

    private async Task RevokeInvitation(TaskGroupInvitationDto invitation)
    {
        try
        {
            await TaskGroupAppService.RevokeInvitationAsync(TaskGroupId, invitation.Id);
            Snackbar.Add(L["InvitationRevoked"], Severity.Success);
            await LoadInvitationsAsync();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    protected override Task HandleErrorAsync(Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Snackbar.Add(L["AnErrorOccurred"], Severity.Error);
        return Task.CompletedTask;
    }
}
