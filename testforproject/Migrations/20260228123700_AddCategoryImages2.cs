using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testforproject.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryImages2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRxoudo-kmIpvw6ATdSFlKh03M2tIMw1P6Jbw&s");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "https://www.google.com/imgres?q=Gaming%20%26%20eSports&imgurl=https%3A%2F%2Fluhisummercamps.org%2Fwp-content%2Fuploads%2F2023%2F07%2Fheader-esports.jpg&imgrefurl=https%3A%2F%2Fluhisummercamps.org%2Fthe-benefits-of-competitive-gaming-and-esports%2F&docid=Wpg71i9teQo2KM&tbnid=MnZ1L84ExUZNGM&vet=12ahUKEwj4q-PxlvySAxV_iKgCHRQzA3AQnPAOegQIGRAB..i&w=1400&h=933&hcb=2&ved=2ahUKEwj4q-PxlvySAxV_iKgCHRQzA3AQnPAOegQIGRAB");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ImageUrl", "Name" },
                values: new object[] { 3, null, "Finance & Investing" });
        }
    }
}
