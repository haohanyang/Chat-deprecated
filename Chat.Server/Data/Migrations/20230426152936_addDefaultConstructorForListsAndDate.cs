using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class addDefaultConstructorForListsAndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Time",
                table: "UserMessage",
                newName: "SentTime");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "GroupMessage",
                newName: "SentTime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedTime",
                table: "Membership",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SentTime",
                table: "UserMessage",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "SentTime",
                table: "GroupMessage",
                newName: "Time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedTime",
                table: "Membership",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
