using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracking.Migrations
{
    /// <inheritdoc />
    public partial class RecurrencePattern_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecurrencePattern_DaysOfWeek",
                table: "TaskTrackingTaskItems");

            migrationBuilder.AddColumn<int>(
                name: "RecurrencePattern_DaysOfWeekFlags",
                table: "TaskTrackingTaskItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecurrencePattern_DaysOfWeekFlags",
                table: "TaskTrackingTaskItems");

            migrationBuilder.AddColumn<string>(
                name: "RecurrencePattern_DaysOfWeek",
                table: "TaskTrackingTaskItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
