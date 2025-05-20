using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskTracking;

public static class GoTrackDbContextExtensions
{
    public static void ConfigureGoTrackEntities(this ModelBuilder builder)
    {
    }

    public static void ToTaskTrackingTable<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder) where TEntity : class
    {
        entityTypeBuilder.ToTable(TaskTrackingConsts.DbTablePrefix + entityTypeBuilder.Metadata.ClrType.Name + "s",
            TaskTrackingConsts.DbSchema);
    }
}