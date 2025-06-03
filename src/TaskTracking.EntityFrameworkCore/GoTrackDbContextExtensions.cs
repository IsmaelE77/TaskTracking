using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskGroupInvitations;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.Notifications;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace TaskTracking;

public static class GoTrackDbContextExtensions
{
    public static void ConfigureTaskTrackingEntities(this ModelBuilder builder)
    {
        builder.Entity<TaskGroup>(b =>
        {
            b.ToTaskTrackingTable();
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(TaskGroupConsts.MaxTitleLength);
            b.Property(x => x.Description).IsRequired().HasMaxLength(TaskGroupConsts.MaxDescriptionLength);

            b.HasMany(x => x.Tasks)
                .WithOne(x => x.TaskGroup)
                .HasForeignKey(x => x.TaskGroupId);

            b.HasMany(x => x.UserTaskGroups)
                .WithOne(x => x.TaskGroup)
                .HasForeignKey(x => x.TaskGroupId);

            b.HasMany(x => x.Invitations)
                .WithOne(x => x.TaskGroup)
                .HasForeignKey(x => x.TaskGroupId);
        });

        builder.Entity<TaskItem>(b =>
        {
            b.ToTaskTrackingTable();
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(TaskItemConsts.MaxTitleLength);
            b.Property(x => x.Description).IsRequired().HasMaxLength(TaskItemConsts.MaxDescriptionLength);

            b.HasMany(x => x.UserProgresses)
                .WithOne(x => x.TaskItem)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            b.OwnsOne(x => x.RecurrencePattern);

        });

        builder.Entity<TaskGroupInvitation>(b =>
        {
            b.ToTaskTrackingTable();
            b.ConfigureByConvention();

            b.Property(x => x.InvitationCode).IsRequired().HasMaxLength(TaskGroupInvitationConsts.InvitationCodeLength);
            b.HasIndex(x => x.InvitationCode).IsUnique();
            b.HasIndex(x => new { x.TaskGroupId, x.IsUsed, x.ExpirationDate });

            b.HasOne(x => x.UsedByUser)
                .WithMany()
                .HasForeignKey(x => x.UsedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<UserTaskGroup>(b =>
        {
            b.ToTaskTrackingTable();
            b.ConfigureByConvention();
        });

        builder.Entity<UserTaskProgress>(b =>
        {
            b.ToTaskTrackingTable();
            b.ConfigureByConvention();

            b.HasOne(x => x.TaskItem)
                .WithMany(x => x.UserProgresses)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade); // 💥

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.OwnsMany(x => x.ProgressEntries);
        });
    }

    public static void ToTaskTrackingTable<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder)
        where TEntity : class
    {
        entityTypeBuilder.ToTable(TaskTrackingConsts.DbTablePrefix + entityTypeBuilder.Metadata.ClrType.Name + "s",
            TaskTrackingConsts.DbSchema);
    }
}