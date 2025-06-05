namespace TaskTracking;

public static class TaskTrackingDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
    /* Task Group Error Codes */
    public const string InvalidDateRange = "TaskTracking:00001";
    public const string TaskEndDateExceedsGroupEndDate = "TaskTracking:00002";
    public const string CannotCompleteGroupWithIncompleteTasks = "TaskTracking:00003";

    /* Task Item Error Codes */
    public const string CannotSetRecurrencePatternForOneTimeTask = "TaskTracking:00101";
    public const string RecurrencePatternRequired = "TaskTracking:00102";
    public const string AlreadyRecorded = "TaskTracking:00103";
    public const string ProgressDateInFuture = "TaskTracking:00104"; 

    /* Recurrence Pattern Error Codes */
    public const string InvalidRecurrenceInterval = "TaskTracking:00201";
    public const string InvalidRecurrenceOccurrences = "TaskTracking:00202";
    public const string WeeklyRecurrenceRequiresDaysOfWeek = "TaskTracking:00203";
    public const string RecurrenceEndDateExceedsTaskItemEndDate = "TaskTracking:00204";
    public const string RecurrenceMustHaveEndDateOrOccurrences = "TaskTracking:00205";

    /* User Task Group Error Codes */
    public const string CannotRemoveOwner = "TaskTracking:00301";
    public const string UserAlreadyInGroup = "TaskTracking:00302";

    /* User Task Progress Error Codes */
    public const string InvalidProgressPercentage = "TaskTracking:00401";
    public const string UserNotInGroup = "TaskTracking:00402";
    public const string TaskNotInGroup = "TaskTracking:00403";
    public const string ProgressAlreadyExists = "TaskTracking:00404";
    public const string ProgressNotFound = "TaskTracking:00405";
    public const string CannotChangeOwnerRole = "TaskTracking:00406";
    public const string CannotChangeToOwnerRole = "TaskTracking:00407";

    /* Task Group Invitation Error Codes */
    public const string InvitationNotFound = "TaskTracking:00501";
    public const string InvitationExpired = "TaskTracking:00502";
    public const string InvitationAlreadyUsed = "TaskTracking:00503";
    public const string InvitationMaxUsesReached = "TaskTracking:00504";
    public const string InvalidInvitationToken = "TaskTracking:00505";
    public const string CannotGenerateInvitationForInactiveGroup = "TaskTracking:00506";
}