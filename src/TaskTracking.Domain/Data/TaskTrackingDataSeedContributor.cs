using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace TaskTracking.Data;

public class TaskTrackingDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ITaskGroupManager _taskGroupManager;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IRepository<TaskGroup, Guid> _taskGroupRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<TaskTrackingDataSeedContributor> _logger;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IDataFilter _dataFilter;

    // Sample user IDs for reference
    private Guid _adminUserId;
    private Guid _regularUser1Id;
    private Guid _regularUser2Id;
    private Guid _regularUser3Id;

    // Sample task group IDs for reference
    private Guid _workTaskGroupId;
    private Guid _personalTaskGroupId;
    private Guid _teamProjectTaskGroupId;
    private Guid _longTermGoalsTaskGroupId;

    public TaskTrackingDataSeedContributor(
        ITaskGroupManager taskGroupManager,
        IIdentityUserRepository userRepository,
        IRepository<TaskGroup, Guid> taskGroupRepository,
        IGuidGenerator guidGenerator,
        ILogger<TaskTrackingDataSeedContributor> logger, IUnitOfWorkManager unitOfWorkManager, IDataFilter dataFilter)
    {
        _taskGroupManager = taskGroupManager;
        _userRepository = userRepository;
        _taskGroupRepository = taskGroupRepository;
        _guidGenerator = guidGenerator;
        _logger = logger;
        _unitOfWorkManager = unitOfWorkManager;
        _dataFilter = dataFilter;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        _logger.LogInformation("Starting TaskTracking data seeding...");

        // Get or create sample users
        await GetOrCreateSampleUsersAsync();
        await _unitOfWorkManager.Current?.SaveChangesAsync()!;

        // Create sample task groups
        await CreateSampleTaskGroupsAsync();
        await _unitOfWorkManager.Current.SaveChangesAsync();

        // Create sample task items
        await CreateSampleTaskItemsAsync();
        await _unitOfWorkManager.Current.SaveChangesAsync();

        // Add users to task groups
        await AddUsersToTaskGroupsAsync();
        await _unitOfWorkManager.Current.SaveChangesAsync();

        // Create sample progress entries
        await CreateSampleProgressEntriesAsync();
        await _unitOfWorkManager.Current.SaveChangesAsync();

        _logger.LogInformation("TaskTracking data seeding completed successfully.");
    }

    private async Task GetOrCreateSampleUsersAsync()
    {
        _logger.LogInformation("Getting or creating sample users...");

        // Get admin user
        var adminUser = await _userRepository.FindByNormalizedUserNameAsync("ADMIN");
        _adminUserId = adminUser?.Id ?? _guidGenerator.Create();

        // Get or create regular users
        var regularUser1 = await _userRepository.FindByNormalizedUserNameAsync("JOHN.DOE");
        if (regularUser1 == null)
        {
            _regularUser1Id = _guidGenerator.Create();
            regularUser1 = new IdentityUser(
                _regularUser1Id,
                "john.doe",
                "john.doe@example.com");

            await _userRepository.InsertAsync(regularUser1);
            _logger.LogInformation("Created sample user: John Doe");
        }
        else
        {
            _regularUser1Id = regularUser1.Id;
        }

        var regularUser2 = await _userRepository.FindByNormalizedUserNameAsync("JANE.SMITH");
        if (regularUser2 == null)
        {
            _regularUser2Id = _guidGenerator.Create();
            regularUser2 = new IdentityUser(
                _regularUser2Id,
                "jane.smith",
                "jane.smith@example.com");

            await _userRepository.InsertAsync(regularUser2);
            _logger.LogInformation("Created sample user: Jane Smith");
        }
        else
        {
            _regularUser2Id = regularUser2.Id;
        }

        var regularUser3 = await _userRepository.FindByNormalizedUserNameAsync("ALEX.JOHNSON");
        if (regularUser3 == null)
        {
            _regularUser3Id = _guidGenerator.Create();
            regularUser3 = new IdentityUser(
                _regularUser3Id,
                "alex.johnson",
                "alex.johnson@example.com");

            await _userRepository.InsertAsync(regularUser3);
            _logger.LogInformation("Created sample user: Alex Johnson");
        }
        else
        {
            _regularUser3Id = regularUser3.Id;
        }
    }

    private async Task CreateSampleTaskGroupsAsync()
    {
        _logger.LogInformation("Creating sample task groups...");

        // Check if task groups already exist
        var existingGroups = await _taskGroupRepository.GetListAsync();
        if (existingGroups.Any())
        {
            _logger.LogInformation("Sample task groups already exist. Skipping creation.");

            // Store existing group IDs for reference
            var workGroup = existingGroups.FirstOrDefault(g => g.Title == "Work Tasks");
            if (workGroup != null) _workTaskGroupId = workGroup.Id;

            var personalGroup = existingGroups.FirstOrDefault(g => g.Title == "Personal Tasks");
            if (personalGroup != null) _personalTaskGroupId = personalGroup.Id;

            var teamProjectGroup = existingGroups.FirstOrDefault(g => g.Title == "Team Project");
            if (teamProjectGroup != null) _teamProjectTaskGroupId = teamProjectGroup.Id;

            var longTermGoalsGroup = existingGroups.FirstOrDefault(g => g.Title == "Long-term Goals");
            if (longTermGoalsGroup != null) _longTermGoalsTaskGroupId = longTermGoalsGroup.Id;

            return;
        }

        // 1. Work Tasks - with end date
        var workTaskGroup = await _taskGroupManager.CreateAsync(
            "Work Tasks",
            "Daily work-related tasks and responsibilities",
            DateTime.Now.Date,
            DateTime.Now.Date.AddMonths(3),
            _adminUserId);
        _workTaskGroupId = workTaskGroup.Id;
        _logger.LogInformation($"Created task group: Work Tasks (ID: {_workTaskGroupId})");

        // 2. Personal Tasks - without end date
        var personalTaskGroup = await _taskGroupManager.CreateAsync(
            "Personal Tasks",
            "Personal to-do items and reminders",
            DateTime.Now.Date,
            null,
            _regularUser1Id);
        _personalTaskGroupId = personalTaskGroup.Id;
        _logger.LogInformation($"Created task group: Personal Tasks (ID: {_personalTaskGroupId})");

        // 3. Team Project - with end date
        var teamProjectTaskGroup = await _taskGroupManager.CreateAsync(
            "Team Project",
            "Collaborative project with deadlines and milestones",
            DateTime.Now.Date,
            DateTime.Now.Date.AddMonths(6),
            _regularUser2Id);
        _teamProjectTaskGroupId = teamProjectTaskGroup.Id;
        _logger.LogInformation($"Created task group: Team Project (ID: {_teamProjectTaskGroupId})");

        // 4. Long-term Goals - without end date
        var longTermGoalsTaskGroup = await _taskGroupManager.CreateAsync(
            "Long-term Goals",
            "Strategic objectives and long-term planning",
            DateTime.Now.Date,
            null,
            _regularUser3Id);
        _longTermGoalsTaskGroupId = longTermGoalsTaskGroup.Id;
        _logger.LogInformation($"Created task group: Long-term Goals (ID: {_longTermGoalsTaskGroupId})");
    }

    private async Task CreateSampleTaskItemsAsync()
    {
        _logger.LogInformation("Creating sample task items...");

        // Work Tasks Group - One-time tasks
        await _taskGroupManager.CreateTaskItemAsync(
            _workTaskGroupId,
            "Complete quarterly report",
            "Finalize and submit the Q2 financial report",
            DateTime.Now.Date,
            DateTime.Now.Date.AddDays(7),
            TaskType.OneTime);

        await _taskGroupManager.CreateTaskItemAsync(
            _workTaskGroupId,
            "Client meeting preparation",
            "Prepare presentation and materials for the client meeting",
            DateTime.Now.Date.AddDays(1),
            DateTime.Now.Date.AddDays(3),
            TaskType.OneTime);

        // Work Tasks Group - Recurring tasks
        var dailyStandupRecurrence = RecurrencePattern.CreateDaily(1, DateTime.Now.Date.AddMonths(3));
        await _taskGroupManager.CreateTaskItemAsync(
            _workTaskGroupId,
            "Daily standup meeting",
            "Attend the daily team standup meeting",
            DateTime.Now.Date,
            null,
            TaskType.Recurring,
            dailyStandupRecurrence);

        var weeklyReportRecurrence = RecurrencePattern.CreateWeekly(
            1,
            new List<DayOfWeek> { DayOfWeek.Friday },
            DateTime.Now.Date.AddMonths(3));
        await _taskGroupManager.CreateTaskItemAsync(
            _workTaskGroupId,
            "Weekly status report",
            "Submit weekly progress report to the team lead",
            DateTime.Now.Date.AddDays(GetDaysUntilNextDayOfWeek(DayOfWeek.Friday)),
            null,
            TaskType.Recurring,
            weeklyReportRecurrence);

        // Personal Tasks Group - One-time tasks
        await _taskGroupManager.CreateTaskItemAsync(
            _personalTaskGroupId,
            "Grocery shopping",
            "Buy groceries for the week",
            DateTime.Now.Date,
            DateTime.Now.Date.AddDays(1),
            TaskType.OneTime);

        await _taskGroupManager.CreateTaskItemAsync(
            _personalTaskGroupId,
            "Dentist appointment",
            "Regular dental checkup",
            DateTime.Now.Date.AddDays(14),
            DateTime.Now.Date.AddDays(14),
            TaskType.OneTime);

        // Personal Tasks Group - Recurring tasks
        var weeklyWorkoutRecurrence = RecurrencePattern.CreateWeekly(
            1,
            new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
            null);
        await _taskGroupManager.CreateTaskItemAsync(
            _personalTaskGroupId,
            "Workout session",
            "Regular exercise routine",
            DateTime.Now.Date.AddDays(GetDaysUntilNextDayOfWeek(DayOfWeek.Monday)),
            null,
            TaskType.Recurring,
            weeklyWorkoutRecurrence);

        var monthlyBudgetRecurrence = RecurrencePattern.CreateMonthly(1, null);
        await _taskGroupManager.CreateTaskItemAsync(
            _personalTaskGroupId,
            "Monthly budget review",
            "Review and adjust monthly budget",
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1),
            null,
            TaskType.Recurring,
            monthlyBudgetRecurrence);

        // Team Project Group - One-time tasks with dependencies
        await _taskGroupManager.CreateTaskItemAsync(
            _teamProjectTaskGroupId,
            "Project kickoff meeting",
            "Initial team meeting to discuss project scope and responsibilities",
            DateTime.Now.Date.AddDays(3),
            DateTime.Now.Date.AddDays(3),
            TaskType.OneTime);

        await _taskGroupManager.CreateTaskItemAsync(
            _teamProjectTaskGroupId,
            "Requirements gathering",
            "Collect and document project requirements from stakeholders",
            DateTime.Now.Date.AddDays(4),
            DateTime.Now.Date.AddDays(10),
            TaskType.OneTime);

        await _taskGroupManager.CreateTaskItemAsync(
            _teamProjectTaskGroupId,
            "Design phase",
            "Create system architecture and design documents",
            DateTime.Now.Date.AddDays(11),
            DateTime.Now.Date.AddDays(25),
            TaskType.OneTime);

        await _taskGroupManager.CreateTaskItemAsync(
            _teamProjectTaskGroupId,
            "Implementation phase",
            "Develop the solution based on design documents",
            DateTime.Now.Date.AddDays(26),
            DateTime.Now.Date.AddDays(60),
            TaskType.OneTime);

        await _taskGroupManager.CreateTaskItemAsync(
            _teamProjectTaskGroupId,
            "Testing phase",
            "Perform quality assurance and testing",
            DateTime.Now.Date.AddDays(61),
            DateTime.Now.Date.AddDays(75),
            TaskType.OneTime);

        await _taskGroupManager.CreateTaskItemAsync(
            _teamProjectTaskGroupId,
            "Deployment",
            "Deploy the solution to production",
            DateTime.Now.Date.AddDays(76),
            DateTime.Now.Date.AddDays(80),
            TaskType.OneTime);

        // Team Project Group - Recurring tasks
        var biWeeklyStatusRecurrence = RecurrencePattern.CreateWeekly(
            2,
            new List<DayOfWeek> { DayOfWeek.Monday },
            DateTime.Now.Date.AddMonths(6));
        await _taskGroupManager.CreateTaskItemAsync(
            _teamProjectTaskGroupId,
            "Bi-weekly status meeting",
            "Team meeting to discuss progress and blockers",
            DateTime.Now.Date.AddDays(GetDaysUntilNextDayOfWeek(DayOfWeek.Monday)),
            null,
            TaskType.Recurring,
            biWeeklyStatusRecurrence);

        // Long-term Goals Group - One-time tasks with far future dates
        await _taskGroupManager.CreateTaskItemAsync(
            _longTermGoalsTaskGroupId,
            "Career development plan",
            "Create a 5-year career development plan",
            DateTime.Now.Date.AddDays(7),
            DateTime.Now.Date.AddDays(30),
            TaskType.OneTime);

        await _taskGroupManager.CreateTaskItemAsync(
            _longTermGoalsTaskGroupId,
            "Learn a new programming language",
            "Complete an online course for a new programming language",
            DateTime.Now.Date,
            DateTime.Now.Date.AddMonths(6),
            TaskType.OneTime);

        await _taskGroupManager.CreateTaskItemAsync(
            _longTermGoalsTaskGroupId,
            "Professional certification",
            "Obtain industry certification in relevant field",
            DateTime.Now.Date.AddMonths(1),
            DateTime.Now.Date.AddMonths(12),
            TaskType.OneTime);

        // Long-term Goals Group - Recurring tasks for tracking progress
        var monthlyGoalReviewRecurrence = RecurrencePattern.CreateMonthly(1, null);
        await _taskGroupManager.CreateTaskItemAsync(
            _longTermGoalsTaskGroupId,
            "Monthly goals review",
            "Review progress on long-term goals and adjust as needed",
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15).AddMonths(1),
            null,
            TaskType.Recurring,
            monthlyGoalReviewRecurrence);

        var quarterlySkillAssessmentRecurrence = RecurrencePattern.CreateMonthly(3, null);
        await _taskGroupManager.CreateTaskItemAsync(
            _longTermGoalsTaskGroupId,
            "Quarterly skill assessment",
            "Evaluate current skills and identify areas for improvement",
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(3),
            null,
            TaskType.Recurring,
            quarterlySkillAssessmentRecurrence);

        _logger.LogInformation("Sample task items created successfully.");
    }

    private async Task AddUsersToTaskGroupsAsync()
    {
        _logger.LogInformation("Adding users to task groups...");

        // Work Tasks Group - Add all users with different roles
        await _taskGroupManager.AddUserToGroupAsync(_workTaskGroupId, _regularUser1Id, UserTaskGroupRole.CoOwner);
        await _taskGroupManager.AddUserToGroupAsync(_workTaskGroupId, _regularUser2Id, UserTaskGroupRole.Subscriber);
        await _taskGroupManager.AddUserToGroupAsync(_workTaskGroupId, _regularUser3Id, UserTaskGroupRole.Subscriber);

        // Personal Tasks Group - Add one additional user
        await _taskGroupManager.AddUserToGroupAsync(_personalTaskGroupId, _adminUserId, UserTaskGroupRole.Subscriber);

        // Team Project Group - Add all users with different roles
        await _taskGroupManager.AddUserToGroupAsync(_teamProjectTaskGroupId, _adminUserId, UserTaskGroupRole.CoOwner);
        await _taskGroupManager.AddUserToGroupAsync(_teamProjectTaskGroupId, _regularUser1Id, UserTaskGroupRole.CoOwner);
        await _taskGroupManager.AddUserToGroupAsync(_teamProjectTaskGroupId, _regularUser3Id, UserTaskGroupRole.Subscriber);

        // Long-term Goals Group - Add one additional user
        await _taskGroupManager.AddUserToGroupAsync(_longTermGoalsTaskGroupId, _regularUser1Id, UserTaskGroupRole.Subscriber);

        _logger.LogInformation("Users added to task groups successfully.");
    }

    private async Task CreateSampleProgressEntriesAsync()
    {
        _logger.LogInformation("Creating sample progress entries...");

        // Get task groups with details
        var workTaskGroup = await _taskGroupManager.GetWithDetailsAsync(_workTaskGroupId);
        var personalTaskGroup = await _taskGroupManager.GetWithDetailsAsync(_personalTaskGroupId);
        var teamProjectTaskGroup = await _taskGroupManager.GetWithDetailsAsync(_teamProjectTaskGroupId);
        var longTermGoalsTaskGroup = await _taskGroupManager.GetWithDetailsAsync(_longTermGoalsTaskGroupId);

        // Work Tasks Group - Various progress states
        var workTasks = workTaskGroup.Tasks.ToList();
        if (workTasks.Count >= 4)
        {
            // Complete the first task for admin
            await _taskGroupManager.UpdateProgressAsync(
                _workTaskGroupId,
                workTasks[0].Id,
                _adminUserId,
                100,
                "Completed ahead of schedule");

            // Partial progress on the second task for admin
            await _taskGroupManager.UpdateProgressAsync(
                _workTaskGroupId,
                workTasks[1].Id,
                _adminUserId,
                50,
                "Working on the presentation slides");

            // Complete the first task for regularUser1
            await _taskGroupManager.UpdateProgressAsync(
                _workTaskGroupId,
                workTasks[0].Id,
                _regularUser1Id,
                100,
                "Reviewed and approved");

            // Partial progress on the third task for regularUser2
            await _taskGroupManager.UpdateProgressAsync(
                _workTaskGroupId,
                workTasks[2].Id,
                _regularUser2Id,
                25,
                "Started preparation for the meeting");
        }

        // Personal Tasks Group - Various progress states
        var personalTasks = personalTaskGroup.Tasks.ToList();
        if (personalTasks.Count >= 4)
        {
            // Complete the first task for regularUser1
            await _taskGroupManager.UpdateProgressAsync(
                _personalTaskGroupId,
                personalTasks[0].Id,
                _regularUser1Id,
                100,
                "Groceries purchased");

            // Partial progress on the third task for regularUser1
            await _taskGroupManager.UpdateProgressAsync(
                _personalTaskGroupId,
                personalTasks[2].Id,
                _regularUser1Id,
                75,
                "Completed 3 out of 4 workout sessions this week");

        }

        // Team Project Group - Various progress states
        var teamProjectTasks = teamProjectTaskGroup.Tasks.ToList();
        if (teamProjectTasks.Count >= 7)
        {
            // Complete the first task for all team members
            await _taskGroupManager.UpdateProgressAsync(
                _teamProjectTaskGroupId,
                teamProjectTasks[0].Id,
                _regularUser2Id,
                100,
                "Meeting completed, minutes distributed");

            await _taskGroupManager.UpdateProgressAsync(
                _teamProjectTaskGroupId,
                teamProjectTasks[0].Id,
                _adminUserId,
                100,
                "Attended and contributed to discussion");

            await _taskGroupManager.UpdateProgressAsync(
                _teamProjectTaskGroupId,
                teamProjectTasks[0].Id,
                _regularUser1Id,
                100,
                "Presented project overview");

            await _taskGroupManager.UpdateProgressAsync(
                _teamProjectTaskGroupId,
                teamProjectTasks[0].Id,
                _regularUser3Id,
                100,
                "Took meeting notes");

            // Partial progress on the second task for different users
            await _taskGroupManager.UpdateProgressAsync(
                _teamProjectTaskGroupId,
                teamProjectTasks[1].Id,
                _regularUser2Id,
                60,
                "Collected requirements from marketing team");

            await _taskGroupManager.UpdateProgressAsync(
                _teamProjectTaskGroupId,
                teamProjectTasks[1].Id,
                _regularUser1Id,
                40,
                "Working on technical requirements");
        }

        // Long-term Goals Group - Various progress states
        var longTermTasks = longTermGoalsTaskGroup.Tasks.ToList();
        if (longTermTasks.Count >= 5)
        {
            // Partial progress on the first task for the owner
            await _taskGroupManager.UpdateProgressAsync(
                _longTermGoalsTaskGroupId,
                longTermTasks[0].Id,
                _regularUser3Id,
                30,
                "Started drafting career goals");

            // Partial progress on the second task for the owner
            await _taskGroupManager.UpdateProgressAsync(
                _longTermGoalsTaskGroupId,
                longTermTasks[1].Id,
                _regularUser3Id,
                15,
                "Completed first module of the course");

            // Partial progress on the first task for the subscriber
            await _taskGroupManager.UpdateProgressAsync(
                _longTermGoalsTaskGroupId,
                longTermTasks[0].Id,
                _regularUser1Id,
                20,
                "Researching career paths");
        }

        _logger.LogInformation("Sample progress entries created successfully.");
    }

    private int GetDaysUntilNextDayOfWeek(DayOfWeek dayOfWeek)
    {
        int today = (int)DateTime.Now.DayOfWeek;
        int target = (int)dayOfWeek;

        if (today <= target)
            return target - today;
        else
            return 7 - (today - target);
    }
}