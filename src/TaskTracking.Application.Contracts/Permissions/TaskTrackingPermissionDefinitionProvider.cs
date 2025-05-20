using TaskTracking.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace TaskTracking.Permissions;

public class TaskTrackingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(TaskTrackingPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(TaskTrackingPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TaskTrackingResource>(name);
    }
}
