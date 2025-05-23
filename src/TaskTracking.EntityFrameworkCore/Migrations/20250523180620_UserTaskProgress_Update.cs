using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracking.Migrations
{
    /// <inheritdoc />
    public partial class UserTaskProgress_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TaskTrackingUserTaskProgresss");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TaskTrackingUserTaskProgresss");

            migrationBuilder.CreateTable(
                name: "ProgressEntry",
                columns: table => new
                {
                    UserTaskProgressId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressEntry", x => new { x.UserTaskProgressId, x.Id });
                    table.ForeignKey(
                        name: "FK_ProgressEntry_TaskTrackingUserTaskProgresss_UserTaskProgress~",
                        column: x => x.UserTaskProgressId,
                        principalTable: "TaskTrackingUserTaskProgresss",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressEntry");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TaskTrackingUserTaskProgresss",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TaskTrackingUserTaskProgresss",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
