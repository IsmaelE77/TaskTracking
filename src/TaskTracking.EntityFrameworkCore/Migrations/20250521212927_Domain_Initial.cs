using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracking.Migrations
{
    /// <inheritdoc />
    public partial class Domain_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskTrackingTaskGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTrackingTaskGroups", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaskTrackingTaskItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TaskType = table.Column<int>(type: "int", nullable: false),
                    RecurrencePattern_RecurrenceType = table.Column<int>(type: "int", nullable: true),
                    RecurrencePattern_Interval = table.Column<int>(type: "int", nullable: true),
                    RecurrencePattern_DaysOfWeek = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecurrencePattern_EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RecurrencePattern_Occurrences = table.Column<int>(type: "int", nullable: true),
                    TaskGroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaskTrackingUserTaskGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TaskGroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
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
                        name: "FK_TaskTrackingUserTaskGroups_TaskTrackingTaskGroups_TaskGroupId",
                        column: x => x.TaskGroupId,
                        principalTable: "TaskTrackingTaskGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaskTrackingUserTaskProgresss",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TaskItemId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserTaskGroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserTaskGroupId1 = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                        name: "FK_TaskTrackingUserTaskProgresss_TaskTrackingTaskItems_TaskItem~",
                        column: x => x.TaskItemId,
                        principalTable: "TaskTrackingTaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTrackingUserTaskProgresss_TaskTrackingUserTaskGroups_Use~",
                        column: x => x.UserTaskGroupId,
                        principalTable: "TaskTrackingUserTaskGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTrackingUserTaskProgresss_TaskTrackingUserTaskGroups_Us~1",
                        column: x => x.UserTaskGroupId1,
                        principalTable: "TaskTrackingUserTaskGroups",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingUserTaskProgresss_UserTaskGroupId",
                table: "TaskTrackingUserTaskProgresss",
                column: "UserTaskGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingUserTaskProgresss_UserTaskGroupId1",
                table: "TaskTrackingUserTaskProgresss",
                column: "UserTaskGroupId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskTrackingUserTaskProgresss");

            migrationBuilder.DropTable(
                name: "TaskTrackingTaskItems");

            migrationBuilder.DropTable(
                name: "TaskTrackingUserTaskGroups");

            migrationBuilder.DropTable(
                name: "TaskTrackingTaskGroups");
        }
    }
}
