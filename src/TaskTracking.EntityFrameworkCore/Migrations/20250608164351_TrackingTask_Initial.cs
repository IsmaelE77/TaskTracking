using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TaskTracking.Migrations
{
    /// <inheritdoc />
    public partial class TrackingTask_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskTrackingTaskGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTrackingTaskGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskTrackingTaskGroupInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitationToken = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxUses = table.Column<int>(type: "integer", nullable: false),
                    CurrentUses = table.Column<int>(type: "integer", nullable: false),
                    DefaultRole = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTrackingTaskGroupInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTrackingTaskGroupInvitations_AbpUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskTrackingTaskGroupInvitations_TaskTrackingTaskGroups_Tas~",
                        column: x => x.TaskGroupId,
                        principalTable: "TaskTrackingTaskGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTrackingTaskItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaskType = table.Column<int>(type: "integer", nullable: false),
                    RecurrencePattern_RecurrenceType = table.Column<int>(type: "integer", nullable: true),
                    RecurrencePattern_Interval = table.Column<int>(type: "integer", nullable: true),
                    RecurrencePattern_DaysOfWeekFlags = table.Column<int>(type: "integer", nullable: true),
                    RecurrencePattern_EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecurrencePattern_Occurrences = table.Column<int>(type: "integer", nullable: true),
                    TaskGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTrackingTaskItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTrackingTaskItems_TaskTrackingTaskGroups_TaskGroupId",
                        column: x => x.TaskGroupId,
                        principalTable: "TaskTrackingTaskGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTrackingUserTaskGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTrackingUserTaskGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTrackingUserTaskGroups_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTrackingUserTaskGroups_TaskTrackingTaskGroups_TaskGroup~",
                        column: x => x.TaskGroupId,
                        principalTable: "TaskTrackingTaskGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTrackingUserTaskProgresss",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgressPercentage = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTrackingUserTaskProgresss", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTrackingUserTaskProgresss_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTrackingUserTaskProgresss_TaskTrackingTaskItems_TaskIte~",
                        column: x => x.TaskItemId,
                        principalTable: "TaskTrackingTaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressEntry",
                columns: table => new
                {
                    UserTaskProgressId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressEntry", x => new { x.UserTaskProgressId, x.Id });
                    table.ForeignKey(
                        name: "FK_ProgressEntry_TaskTrackingUserTaskProgresss_UserTaskProgres~",
                        column: x => x.UserTaskProgressId,
                        principalTable: "TaskTrackingUserTaskProgresss",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingTaskGroupInvitations_CreatedByUserId",
                table: "TaskTrackingTaskGroupInvitations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingTaskGroupInvitations_InvitationToken",
                table: "TaskTrackingTaskGroupInvitations",
                column: "InvitationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingTaskGroupInvitations_TaskGroupId",
                table: "TaskTrackingTaskGroupInvitations",
                column: "TaskGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingTaskItems_TaskGroupId",
                table: "TaskTrackingTaskItems",
                column: "TaskGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingUserTaskGroups_TaskGroupId",
                table: "TaskTrackingUserTaskGroups",
                column: "TaskGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingUserTaskGroups_UserId",
                table: "TaskTrackingUserTaskGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingUserTaskProgresss_TaskItemId",
                table: "TaskTrackingUserTaskProgresss",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingUserTaskProgresss_UserId",
                table: "TaskTrackingUserTaskProgresss",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressEntry");

            migrationBuilder.DropTable(
                name: "TaskTrackingTaskGroupInvitations");

            migrationBuilder.DropTable(
                name: "TaskTrackingUserTaskGroups");

            migrationBuilder.DropTable(
                name: "TaskTrackingUserTaskProgresss");

            migrationBuilder.DropTable(
                name: "TaskTrackingTaskItems");

            migrationBuilder.DropTable(
                name: "TaskTrackingTaskGroups");
        }
    }
}
