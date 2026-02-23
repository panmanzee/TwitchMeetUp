using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testforproject.Migrations
{
    /// <inheritdoc />
    public partial class hack2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Requirements_RequirementsId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_RequirementsId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequirementsId",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequirementsId",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Events_RequirementsId",
                table: "Events",
                column: "RequirementsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Requirements_RequirementsId",
                table: "Events",
                column: "RequirementsId",
                principalTable: "Requirements",
                principalColumn: "RequirementsId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
