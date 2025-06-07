namespace TaskTracking.TaskGroupAggregate.UserTaskGroups;

public static class UserTaskGroupPermissions
{
    public const string GroupName = "TaskGroups";

    public const string Update = GroupName + ".Update";
    public const string Delete = GroupName + ".Delete";
    public const string ManageUsers = GroupName + ".ManageUsers";
    public const string RecordProgress = GroupName + ".RecordProgress";
    public const string ManageTaskItems = GroupName + ".ManageTaskItems";
    public const string CreateTaskItems = GroupName + ".CreateTaskItems";
    public const string UpdateTaskItems = GroupName + ".UpdateTaskItems";
    public const string DeleteTaskItems = GroupName + ".DeleteTaskItems";
    public const string GenerateInvitations = GroupName + ".GenerateInvitations";

}