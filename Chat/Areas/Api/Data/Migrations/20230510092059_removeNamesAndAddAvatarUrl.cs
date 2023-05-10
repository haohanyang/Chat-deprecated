using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Areas.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeNamesAndAddAvatarUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverUsername",
                table: "UserMessages");

            migrationBuilder.DropColumn(
                name: "SenderUsername",
                table: "UserMessages");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "SenderUsername",
                table: "GroupMessages");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverUsername",
                table: "UserMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderUsername",
                table: "UserMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "GroupMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderUsername",
                table: "GroupMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
