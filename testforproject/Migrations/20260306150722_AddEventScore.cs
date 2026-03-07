using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testforproject.Migrations
{
    /// <inheritdoc />
    public partial class AddEventScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventScores_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Eid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventScores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventScores_EventId",
                table: "EventScores",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventScores_UserId",
                table: "EventScores",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventScores");
        }
    }
}
