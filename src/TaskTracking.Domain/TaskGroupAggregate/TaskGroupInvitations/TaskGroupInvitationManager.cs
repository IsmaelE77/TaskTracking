using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TaskTracking.TaskGroupAggregate.TaskGroupInvitations;

public class TaskGroupInvitationManager : DomainService
{
    private readonly IRepository<TaskGroupInvitation, Guid> _invitationRepository;
    private readonly IRepository<TaskGroup, Guid> _taskGroupRepository;
    private readonly IRepository<UserTaskGroup, Guid> _userTaskGroupRepository;

    public TaskGroupInvitationManager(
        IRepository<TaskGroupInvitation, Guid> invitationRepository,
        IRepository<TaskGroup, Guid> taskGroupRepository,
        IRepository<UserTaskGroup, Guid> userTaskGroupRepository)
    {
        _invitationRepository = invitationRepository;
        _taskGroupRepository = taskGroupRepository;
        _userTaskGroupRepository = userTaskGroupRepository;
    }

    public async Task<TaskGroupInvitation> CreateInvitationAsync(
        Guid taskGroupId,
        Guid creatorUserId,
        int? expirationHours = null)
    {
        // Verify the creator is an owner of the task group
        var taskGroup = await _taskGroupRepository.GetAsync(taskGroupId, includeDetails: true);
        var userTaskGroup = taskGroup.UserTaskGroups.FirstOrDefault(utg => utg.UserId == creatorUserId);
        
        if (userTaskGroup == null || userTaskGroup.Role != UserTaskGroupRole.Owner)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.CannotGenerateInvitationForNonOwner);
        }

        // Check if there are too many active invitations
        var activeInvitationsCount = await _invitationRepository.CountAsync(
            i => i.TaskGroupId == taskGroupId && !i.IsUsed && i.ExpirationDate > DateTime.UtcNow);

        if (activeInvitationsCount >= TaskGroupInvitationConsts.MaxActiveInvitationsPerGroup)
        {
            throw new BusinessException("Maximum number of active invitations reached for this task group.");
        }

        var invitationCode = GenerateInvitationCode();
        var expirationDate = DateTime.UtcNow.AddHours(expirationHours ?? TaskGroupInvitationConsts.DefaultExpirationHours);

        var invitation = new TaskGroupInvitation(
            GuidGenerator.Create(),
            taskGroupId,
            invitationCode,
            expirationDate);

        return await _invitationRepository.InsertAsync(invitation);
    }

    public async Task<TaskGroupInvitation> GetValidInvitationAsync(string invitationCode)
    {
        var invitation = await _invitationRepository.FirstOrDefaultAsync(
            i => i.InvitationCode == invitationCode);

        if (invitation == null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvitationNotFound);
        }

        if (invitation.IsUsed)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvitationAlreadyUsed);
        }

        if (invitation.IsExpired())
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.InvitationExpired);
        }

        return invitation;
    }

    public async Task<UserTaskGroup> UseInvitationAsync(
        string invitationCode,
        Guid userId,
        UserTaskGroupRole role = UserTaskGroupRole.Subscriber)
    {
        var invitation = await GetValidInvitationAsync(invitationCode);

        // Check if user is already in the group
        var existingUserTaskGroup = await _userTaskGroupRepository.FirstOrDefaultAsync(
            utg => utg.TaskGroupId == invitation.TaskGroupId && utg.UserId == userId);

        if (existingUserTaskGroup != null)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.UserAlreadyInGroupViaInvitation);
        }

        // Mark invitation as used
        invitation.MarkAsUsed(userId);
        await _invitationRepository.UpdateAsync(invitation);

        // Add user to the task group
        var userTaskGroup = new UserTaskGroup(
            GuidGenerator.Create(),
            userId,
            invitation.TaskGroupId,
            role);

        return await _userTaskGroupRepository.InsertAsync(userTaskGroup);
    }

    public async Task RevokeInvitationAsync(Guid invitationId, Guid requestingUserId)
    {
        var invitation = await _invitationRepository.GetAsync(invitationId, includeDetails: true);
        
        // Verify the requesting user is an owner of the task group
        var taskGroup = await _taskGroupRepository.GetAsync(invitation.TaskGroupId, includeDetails: true);
        var userTaskGroup = taskGroup.UserTaskGroups.FirstOrDefault(utg => utg.UserId == requestingUserId);
        
        if (userTaskGroup == null || userTaskGroup.Role != UserTaskGroupRole.Owner)
        {
            throw new BusinessException(TaskTrackingDomainErrorCodes.CannotGenerateInvitationForNonOwner);
        }

        await _invitationRepository.DeleteAsync(invitation);
    }

    private string GenerateInvitationCode()
    {
        using var rng = RandomNumberGenerator.Create();

        // Generate a longer string to ensure we have enough characters after cleanup
        var result = "";
        while (result.Length < TaskGroupInvitationConsts.InvitationCodeLength)
        {
            var bytes = new byte[32]; // Generate more bytes to account for removed characters
            rng.GetBytes(bytes);
            var base64 = Convert.ToBase64String(bytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");

            result += base64;
        }

        return result.Substring(0, TaskGroupInvitationConsts.InvitationCodeLength);
    }
}
