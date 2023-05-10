using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Areas.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Memberships");

            migrationBuilder.RenameColumn(
                name: "JoinedTime",
                table: "Memberships",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Memberships",
                newName: "JoinedTime");

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
