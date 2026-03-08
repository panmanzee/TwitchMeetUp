using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testforproject.Data.Migrations
{
    /// <inheritdoc />
    public partial class finish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ParticipantConfirmations_EventId",
                table: "ParticipantConfirmations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantConfirmations_UserId",
                table: "ParticipantConfirmations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantConfirmations_Events_EventId",
                table: "ParticipantConfirmations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Eid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantConfirmations_Users_UserId",
                table: "ParticipantConfirmations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Uid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantConfirmations_Events_EventId",
                table: "ParticipantConfirmations");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantConfirmations_Users_UserId",
                table: "ParticipantConfirmations");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantConfirmations_EventId",
                table: "ParticipantConfirmations");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantConfirmations_UserId",
                table: "ParticipantConfirmations");
        }
    }
}
