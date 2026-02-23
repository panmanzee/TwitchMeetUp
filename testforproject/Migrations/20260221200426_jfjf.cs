using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testforproject.Migrations
{
    /// <inheritdoc />
    public partial class jfjf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Requirements_RequirementsId",
                table: "Events");

            migrationBuilder.AlterColumn<int>(
                name: "RequirementsId",
                table: "Events",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Requirements_RequirementsId",
                table: "Events",
                column: "RequirementsId",
                principalTable: "Requirements",
                principalColumn: "RequirementsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Requirements_RequirementsId",
                table: "Events");

            migrationBuilder.AlterColumn<int>(
                name: "RequirementsId",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
