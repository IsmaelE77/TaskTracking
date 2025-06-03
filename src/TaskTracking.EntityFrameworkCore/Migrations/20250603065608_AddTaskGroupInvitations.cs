using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracking.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskGroupInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskTrackingTaskGroupInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TaskGroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InvitationCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UsedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UsedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_TaskTrackingTaskGroupInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTrackingTaskGroupInvitations_AbpUsers_UsedByUserId",
                        column: x => x.UsedByUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaskTrackingTaskGroupInvitations_TaskTrackingTaskGroups_Task~",
                        column: x => x.TaskGroupId,
                        principalTable: "TaskTrackingTaskGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingTaskGroupInvitations_InvitationCode",
                table: "TaskTrackingTaskGroupInvitations",
                column: "InvitationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingTaskGroupInvitations_TaskGroupId_IsUsed_Expirati~",
                table: "TaskTrackingTaskGroupInvitations",
                columns: new[] { "TaskGroupId", "IsUsed", "ExpirationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskTrackingTaskGroupInvitations_UsedByUserId",
                table: "TaskTrackingTaskGroupInvitations",
                column: "UsedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskTrackingTaskGroupInvitations");
        }
    }
}
