using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testforproject.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggerUserToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TriggerUserUid",
                table: "Notifications",
                type: "int",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_TriggerUserUid",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TriggerUserUid",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TriggerUserUid",
                table: "Notifications");
        }
    }
}
