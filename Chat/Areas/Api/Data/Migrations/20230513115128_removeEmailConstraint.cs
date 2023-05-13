using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Areas.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeEmailConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "GroupName",
                table: "Groups",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_GroupName",
                table: "Groups",
                newName: "IX_Groups_Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Groups",
                newName: "GroupName");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                newName: "IX_Groups_GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }
    }
}
