using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoruCevapPortalı.Migrations
{
    /// <inheritdoc />
    public partial class ProfilSekmeleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "About",
                table: "AspNetUsers",
                newName: "AboutMe");

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Reputation",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Reputation",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "AboutMe",
                table: "AspNetUsers",
                newName: "About");
        }
    }
}
