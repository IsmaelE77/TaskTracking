using TaskTracking.Localization;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace TaskTracking.Permissions;

public class TaskTrackingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var taskGroupsPermission = context.AddGroup(
            UserTaskGroupPermissions.GroupName,
            L("Permission:TaskGroups"));

        taskGroupsPermission.AddPermission(
            UserTaskGroupPermissions.Update,
            L("Permission:TaskGroups.Update"));

        taskGroupsPermission.AddPermission(
            UserTaskGroupPermissions.Delete,
            L("Permission:TaskGroups.Delete"));

        taskGroupsPermission.AddPermission(
            UserTaskGroupPermissions.ManageUsers,
            L("Permission:TaskGroups.ManageUsers"));

        taskGroupsPermission.AddPermission(
            UserTaskGroupPermissions.RecordProgress,
            L("Permission:TaskGroups.RecordProgress"));

        taskGroupsPermission.AddPermission(
            UserTaskGroupPermissions.ManageTaskItems,
            L("Permission:TaskGroups.ManageTaskItems"));

        taskGroupsPermission.AddPermission(
            UserTaskGroupPermissions.CreateTaskItems,
            L("Permission:TaskGroups.CreateTaskItems"));

        taskGroupsPermission.AddPermission(
            UserTaskGroupPermissions.UpdateTaskItems,
            L("Permission:TaskGroups.UpdateTaskItems"));

        taskGroupsPermission.AddPermission(
            UserTaskGroupPermissions.DeleteTaskItems,
            L("Permission:TaskGroups.DeleteTaskItems"));

        taskGroupsPermission.AddPermission(
            UserTaskGroupPermissions.GenerateInvitations,
            L("Permission:TaskGroups.GenerateInvitations"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TaskTrackingResource>(name);
    }
}