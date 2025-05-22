using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracking.Migrations
{
    /// <inheritdoc />
    public partial class UserProgresses_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskTrackingUserTaskProgresss_TaskTrackingUserTaskGroups_Use~",
                table: "TaskTrackingUserTaskProgresss");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskTrackingUserTaskProgresss_TaskTrackingUserTaskGroups_Us~1",
                table: "TaskTrackingUserTaskProgresss");

            migrationBuilder.DropIndex(
                name: "IX_TaskTrackingUserTaskProgresss_UserTaskGroupId",
                table: "TaskTrackingUserTaskProgresss");

            migrationBuilder.DropIndex(
                name: "IX_TaskTrackingUserTaskProgresss_UserTaskGroupId1",
                table: "TaskTrackingUserTaskProgresss");

            migrationBuilder.DropColumn(
                name: "UserTaskGroupId",
                table: "TaskTrackingUserTaskProgresss");

            migrationBuilder.DropColumn(
                name: "UserTaskGroupId1",
                table: "TaskTrackingUserTaskProgresss");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserTaskGroupId",
                table: "TaskTrackingUserTaskProgresss",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UserTaskGroupId1",
                table: "TaskTrackingUserTaskProgresss",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingUserTaskProgresss_UserTaskGroupId",
                table: "TaskTrackingUserTaskProgresss",
                column: "UserTaskGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingUserTaskProgresss_UserTaskGroupId1",
                table: "TaskTrackingUserTaskProgresss",
                column: "UserTaskGroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskTrackingUserTaskProgresss_TaskTrackingUserTaskGroups_Use~",
                table: "TaskTrackingUserTaskProgresss",
                column: "UserTaskGroupId",
                principalTable: "TaskTrackingUserTaskGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskTrackingUserTaskProgresss_TaskTrackingUserTaskGroups_Us~1",
                table: "TaskTrackingUserTaskProgresss",
                column: "UserTaskGroupId1",
                principalTable: "TaskTrackingUserTaskGroups",
                principalColumn: "Id");
        }
    }
}
