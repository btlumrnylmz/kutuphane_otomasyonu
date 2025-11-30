using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KutuphaneOtomasyonu.Migrations
{
    /// <inheritdoc />
    public partial class MakeShelfLocationNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Copies_CopyId1",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CopyId1",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CopyId1",
                table: "Reservations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CopyId1",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CopyId1",
                table: "Reservations",
                column: "CopyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Copies_CopyId1",
                table: "Reservations",
                column: "CopyId1",
                principalTable: "Copies",
                principalColumn: "CopyId");
        }
    }
}
