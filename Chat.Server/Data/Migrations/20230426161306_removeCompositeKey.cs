using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessage_AspNetUsers_SenderId",
                table: "GroupMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessage_Groups_ReceiverId",
                table: "GroupMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_Membership_AspNetUsers_UserId",
                table: "Membership");

            migrationBuilder.DropForeignKey(
                name: "FK_Membership_Groups_GroupId",
                table: "Membership");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessage_AspNetUsers_ReceiverId",
                table: "UserMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessage_AspNetUsers_SenderId",
                table: "UserMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMessage",
                table: "UserMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Membership",
                table: "Membership");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupMessage",
                table: "GroupMessage");

            migrationBuilder.RenameTable(
                name: "UserMessage",
                newName: "UserMessages");

            migrationBuilder.RenameTable(
                name: "Membership",
                newName: "Memberships");

            migrationBuilder.RenameTable(
                name: "GroupMessage",
                newName: "GroupMessages");

            migrationBuilder.RenameIndex(
                name: "IX_UserMessage_SenderId",
                table: "UserMessages",
                newName: "IX_UserMessages_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_UserMessage_ReceiverId",
                table: "UserMessages",
                newName: "IX_UserMessages_ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_Membership_GroupId",
                table: "Memberships",
                newName: "IX_Memberships_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMessage_SenderId",
                table: "GroupMessages",
                newName: "IX_GroupMessages_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMessage_ReceiverId",
                table: "GroupMessages",
                newName: "IX_GroupMessages_ReceiverId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Memberships",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMessages",
                table: "UserMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupMessages",
                table: "GroupMessages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserId",
                table: "Memberships",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_AspNetUsers_SenderId",
                table: "GroupMessages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_Groups_ReceiverId",
                table: "GroupMessages",
                column: "ReceiverId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_AspNetUsers_UserId",
                table: "Memberships",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Groups_GroupId",
                table: "Memberships",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessages_AspNetUsers_ReceiverId",
                table: "UserMessages",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessages_AspNetUsers_SenderId",
                table: "UserMessages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_AspNetUsers_SenderId",
                table: "GroupMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_Groups_ReceiverId",
                table: "GroupMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_AspNetUsers_UserId",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Groups_GroupId",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_AspNetUsers_ReceiverId",
                table: "UserMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_AspNetUsers_SenderId",
                table: "UserMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMessages",
                table: "UserMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_UserId",
                table: "Memberships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupMessages",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Memberships");

            migrationBuilder.RenameTable(
                name: "UserMessages",
                newName: "UserMessage");

            migrationBuilder.RenameTable(
                name: "Memberships",
                newName: "Membership");

            migrationBuilder.RenameTable(
                name: "GroupMessages",
                newName: "GroupMessage");

            migrationBuilder.RenameIndex(
                name: "IX_UserMessages_SenderId",
                table: "UserMessage",
                newName: "IX_UserMessage_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_UserMessages_ReceiverId",
                table: "UserMessage",
                newName: "IX_UserMessage_ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_GroupId",
                table: "Membership",
                newName: "IX_Membership_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMessages_SenderId",
                table: "GroupMessage",
                newName: "IX_GroupMessage_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMessages_ReceiverId",
                table: "GroupMessage",
                newName: "IX_GroupMessage_ReceiverId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMessage",
                table: "UserMessage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Membership",
                table: "Membership",
                columns: new[] { "UserId", "GroupId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupMessage",
                table: "GroupMessage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessage_AspNetUsers_SenderId",
                table: "GroupMessage",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessage_Groups_ReceiverId",
                table: "GroupMessage",
                column: "ReceiverId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Membership_AspNetUsers_UserId",
                table: "Membership",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Membership_Groups_GroupId",
                table: "Membership",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessage_AspNetUsers_ReceiverId",
                table: "UserMessage",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessage_AspNetUsers_SenderId",
                table: "UserMessage",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
