using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testforproject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFCFSandTriggerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_TriggerUserUid",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TriggerUserUid",
                table: "Notifications");

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedAt",
                table: "EventUser",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoinedAt",
                table: "EventUser");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TriggerUserUid",
                table: "Notifications",
                column: "TriggerUserUid");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_TriggerUserUid",
                table: "Notifications",
                column: "TriggerUserUid",
                principalTable: "Users",
                principalColumn: "Uid");
        }
    }
}
