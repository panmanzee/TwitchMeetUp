using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace testforproject.Migrations
{
    /// <inheritdoc />
    public partial class hi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requirements",
                columns: table => new
                {
                    RequirementsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParticipantScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requirements", x => x.RequirementsId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Uid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Age = table.Column<long>(type: "bigint", nullable: true),
                    ProfilePictureSrc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HostScore = table.Column<int>(type: "int", nullable: true),
                    ParticipateScore = table.Column<int>(type: "int", nullable: true),
                    PopularityScore = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Users_UserUid",
                        column: x => x.UserUid,
                        principalTable: "Users",
                        principalColumn: "Uid");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Eid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxParticitpant = table.Column<int>(type: "int", nullable: false),
                    ExpiredDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EventStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventStop = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Eid);
                    table.ForeignKey(
                        name: "FK_Events_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Uid");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserUid = table.Column<int>(type: "int", nullable: false),
                    IsReaded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserUid",
                        column: x => x.UserUid,
                        principalTable: "Users",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFollows",
                columns: table => new
                {
                    FollowerUid = table.Column<int>(type: "int", nullable: false),
                    FollowingUid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollows", x => new { x.FollowerUid, x.FollowingUid });
                    table.ForeignKey(
                        name: "FK_UserFollows_Users_FollowerUid",
                        column: x => x.FollowerUid,
                        principalTable: "Users",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFollows_Users_FollowingUid",
                        column: x => x.FollowingUid,
                        principalTable: "Users",
                        principalColumn: "Uid");
                });

            migrationBuilder.CreateTable(
                name: "EventCategories",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "int", nullable: false),
                    EventsEid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCategories", x => new { x.CategoriesId, x.EventsEid });
                    table.ForeignKey(
                        name: "FK_EventCategories_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventCategories_Events_EventsEid",
                        column: x => x.EventsEid,
                        principalTable: "Events",
                        principalColumn: "Eid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventUser",
                columns: table => new
                {
                    Eid = table.Column<int>(type: "int", nullable: false),
                    ParticitpantUid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventUser", x => new { x.Eid, x.ParticitpantUid });
                    table.ForeignKey(
                        name: "FK_EventUser_Events_Eid",
                        column: x => x.Eid,
                        principalTable: "Events",
                        principalColumn: "Eid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventUser_Users_ParticitpantUid",
                        column: x => x.ParticitpantUid,
                        principalTable: "Users",
                        principalColumn: "Uid");
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ImageUrl", "Name", "UserUid" },
                values: new object[,]
                {
                    { 1, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRfY6dGXDvuNyxepwwRCDD4aI8MmrroG4Xj8g&s", "Sports & Fitness", null },
                    { 2, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRxoudo-kmIpvw6ATdSFlKh03M2tIMw1P6Jbw&s", "Gaming & eSports", null },
                    { 4, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTUXSwrWPMPioZYdGqVU1dBR75K0bNYuixisQ&s", "Technology & Coding", null },
                    { 5, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQB_3U8P0eFjlVTUUZtAx2Be8ob_2HiFtH68Q&s", "Education & Learning", null },
                    { 6, "https://voca-land.sgp1.cdn.digitaloceanspaces.com/43844/1723178338611/a55b073aef1b83a4ccf3a83be979de70.jpg", "Arts & Crafts", null },
                    { 7, "https://i.shgcdn.com/3889bf5d-9000-4cbc-a021-cd6051095102/-/format/auto/-/preview/3000x3000/-/quality/lighter/", "Food & Drink", null },
                    { 8, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSWlwvLgv80oIsBgM73ux9uS1qWkrduUPplnQ&s", "Travel & Outdoors", null },
                    { 9, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQbWVzCwBNaRL8UYHtc_4_MOL6ZBeRmdH2S3g&s", "Health & Wellness", null },
                    { 10, "https://www.abundance.global/wp-content/uploads/2024/06/business-networking-1080x675-1.jpeg", "Networking & Business", null },
                    { 11, "https://img.jakpost.net/c/2019/06/12/2019_06_12_74202_1560308728._large.jpg", "Music & Concerts", null },
                    { 12, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQAFJZdr-RTJ5_Sa_unf88P4OIW0dXyYcDcFQ&s", "Movies & Theater", null },
                    { 13, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTdrWNEbhPUO60Zaz7LUDmVBbUmBLEWFX38JA&s", "Photography & Video", null },
                    { 14, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSRJ88He6SLg3xF2KS-CfXE7pZJBo1r4PaHTA&s", "Books & Writing", null },
                    { 15, "https://carollaguirre.wordpress.com/wp-content/uploads/2013/11/tumblr_inline_mm0kys229c1qz4rgp.jpg", "Language & Culture", null },
                    { 16, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRV7uM9mwm9yq3LKp00n1qd0i96m9gpJ-2EUQ&s", "Volunteering & Charity", null },
                    { 17, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSEKp_2e5feC9OmHADhkiPoniNzMpVSE6b7KA&s", "Pets & Animals", null },
                    { 18, "https://mpics.mgronline.com/pics/Images/566000002659701.JPEG", "Fashion & Beauty", null },
                    { 19, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRLSugPQ0OSBDuPK25QJu8BplFKJwDn1zC3vQ&s", "Science & Research", null },
                    { 20, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS5j3y16Pum3NPZThK9FERKVdBzv0jey5-hJA&s", "History & Philosophy", null },
                    { 21, "https://www.focusonthefamily.com/wp-content/uploads/2019/07/D119D43F1A57459B858B9A11EC84408A-1024x575.jpeg", "Parenting & Family", null },
                    { 22, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTsdqbWrGfqPZuuSRdAcknqiofR2ibJoYBWPQ&s", "Spirituality & Beliefs", null },
                    { 23, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ3_kqEOFllhUa_JYjPsbvtdpKzPY6xPrkOgA&s", "Cars & Motorcycles", null },
                    { 24, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQE_Cbgok89HHW9w9ub5PlE9A0bEoJSVzWMZQ&s", "Real Estate", null },
                    { 25, "https://capital-placement.com/wp-content/uploads/2020/12/career-development.png", "Career Development", null },
                    { 26, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ3WKQeAR7thwxBTzYxQQsSRUBJCUSrXleCyQ&s", "Politics & Society", null },
                    { 27, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRbY_kY6LSxZq4oEs_EXUpzua16XDf6QJWqJw&s", "Dancing & Performing", null },
                    { 28, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQvK_JEMhb7owBgmSSTO4P_Z1lvxf36WFp7WQ&s", "Board Games & Tabletop", null },
                    { 29, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSZ9KrCcuANQ3OoN5Ju7OmUCBlI7Tyw8K_Neg&s", "DIY & Home Improvement", null },
                    { 30, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTakHVTgT0jryCuvRX410HBBwTTghRYhXOVYg&s", "Comedy & Improv", null },
                    { 31, "https://earth.org/wp-content/uploads/2023/03/Untitled-683-%C3%97-1024px-1024-%C3%97-683px-73.jpg", "Environment & Nature", null },
                    { 32, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQcdLgyLqF6dX_21oeEwpkjpCOT83E-gnL3Iw&s", "Dating & Singles", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserUid",
                table: "Categories",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_EventCategories_EventsEid",
                table: "EventCategories",
                column: "EventsEid");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OwnerId",
                table: "Events",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_EventUser_ParticitpantUid",
                table: "EventUser",
                column: "ParticitpantUid");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserUid",
                table: "Notifications",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowingUid",
                table: "UserFollows",
                column: "FollowingUid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventCategories");

            migrationBuilder.DropTable(
                name: "EventUser");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Requirements");

            migrationBuilder.DropTable(
                name: "UserFollows");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
