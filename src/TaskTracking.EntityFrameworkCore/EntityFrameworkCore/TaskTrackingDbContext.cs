using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace TaskTracking.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class TaskTrackingDbContext :
    AbpDbContext<TaskTrackingDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    public DbSet<TaskGroup> TaskGroups { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<UserTaskGroup> UserTaskGroups { get; set; }
    public DbSet<UserTaskProgress> UserTaskProgresses { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }

    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public TaskTrackingDbContext(DbContextOptions<TaskTrackingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();
        builder.ConfigureTaskTrackingEntities();

        base.OnModelCreating(builder);
    }

    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        if (typeof(IHaveTaskGroup).IsAssignableFrom(typeof(TEntity)))
            return true;

        if (typeof(IAccessibleTaskGroup).IsAssignableFrom(typeof(TEntity)))
            return true;

        return base.ShouldFilterEntity<TEntity>(entityType);
    }

    protected override Expression<Func<TEntity, bool>>? CreateFilterExpression<TEntity>(ModelBuilder modelBuilder)
    {
        var expression = base.CreateFilterExpression<TEntity>(modelBuilder);

        expression = TryAddTaskGroupFilterExpression(expression);
        expression = TryAddAccessibleTaskGroupFilterExpression(expression);

        return expression;
    }

    private Expression<Func<TEntity, bool>>? TryAddTaskGroupFilterExpression<TEntity>(
        Expression<Func<TEntity, bool>>? expression) where TEntity : class
    {
        if (!typeof(IHaveTaskGroup).IsAssignableFrom(typeof(TEntity)))
        {
            return expression;
        }

        Expression<Func<TEntity, bool>> taskGroupFilterExpression = e =>
            !DataFilter.IsEnabled<IHaveTaskGroup>() ||
            LazyServiceProvider.LazyGetRequiredService<CurrentUserTaskGroups>()
                .GetAccessibleTaskGroupIds().Contains(
                    EF.Property<Guid>(e, nameof(IHaveTaskGroup.TaskGroupId)));

        expression = expression == null
            ? taskGroupFilterExpression
            : QueryFilterExpressionHelper.CombineExpressions(expression, taskGroupFilterExpression);

        return expression;
    }

    private Expression<Func<TEntity, bool>>? TryAddAccessibleTaskGroupFilterExpression<TEntity>(
        Expression<Func<TEntity, bool>>? expression) where TEntity : class
    {
        if (!typeof(IAccessibleTaskGroup).IsAssignableFrom(typeof(TEntity)))
            return expression;

        Expression<Func<TEntity, bool>> accessibleTaskGroupFilterExpression = e =>
            !DataFilter.IsEnabled<IAccessibleTaskGroup>() ||
            LazyServiceProvider.LazyGetRequiredService<CurrentUserTaskGroups>()
                .GetAccessibleTaskGroupIds().Contains(
                    EF.Property<Guid>(e, nameof(IAccessibleTaskGroup.Id)));

        expression = expression == null
            ? accessibleTaskGroupFilterExpression
            : QueryFilterExpressionHelper.CombineExpressions(expression, accessibleTaskGroupFilterExpression);

        return expression;
    }
}