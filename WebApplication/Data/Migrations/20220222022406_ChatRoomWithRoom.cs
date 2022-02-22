using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication.Data.Migrations
{
    public partial class ChatRoomWithRoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "ChatHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistories_RoomId",
                table: "ChatHistories",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatHistories_ChatRooms_RoomId",
                table: "ChatHistories",
                column: "RoomId",
                principalTable: "ChatRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatHistories_ChatRooms_RoomId",
                table: "ChatHistories");

            migrationBuilder.DropIndex(
                name: "IX_ChatHistories_RoomId",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "ChatHistories");
        }
    }
}
