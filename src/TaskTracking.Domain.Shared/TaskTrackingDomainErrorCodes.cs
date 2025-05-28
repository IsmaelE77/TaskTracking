namespace TaskTracking;

public static class TaskTrackingDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
    /* Task Group Error Codes */
    public const string InvalidDateRange = "TrackingTask:00001";
    public const string TaskEndDateExceedsGroupEndDate = "TrackingTask:00002";
    public const string CannotCompleteGroupWithIncompleteTasks = "TrackingTask:00003";

    /* Task Item Error Codes */
    public const string CannotSetRecurrencePatternForOneTimeTask = "TrackingTask:00101";
    public const string RecurrencePatternRequired = "TrackingTask:00102";
    public const string AlreadyRecorded = "TrackingTask:00103";

    /* Recurrence Pattern Error Codes */
    public const string InvalidRecurrenceInterval = "TrackingTask:00201";
    public const string InvalidRecurrenceOccurrences = "TrackingTask:00202";
    public const string WeeklyRecurrenceRequiresDaysOfWeek = "TrackingTask:00203";
    public const string RecurrenceEndDateExceedsTaskItemEndDate = "TrackingTask:00204";

    /* User Task Group Error Codes */
    public const string CannotRemoveOwner = "TrackingTask:00301";
    public const string UserAlreadyInGroup = "TrackingTask:00302";

    /* User Task Progress Error Codes */
    public const string InvalidProgressPercentage = "TrackingTask:00401";
    public const string UserNotInGroup = "TrackingTask:00402";
    public const string TaskNotInGroup = "TrackingTask:00403";
    public const string ProgressAlreadyExists = "TrackingTask:00404";
    public const string ProgressNotFound = "TrackingTask:00405";
    public const string CannotChangeOwnerRole = "TrackingTask:00406";
    public const string CannotChangeToOwnerRole = "TrackingTask:00407";
}