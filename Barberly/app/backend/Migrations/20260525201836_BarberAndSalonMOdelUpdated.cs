using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class BarberAndSalonMOdelUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Salons_salonId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "salonId",
                table: "AspNetUsers",
                newName: "SalonId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_salonId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_SalonId");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Salons",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "Salons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Salons_OwnerId",
                table: "Salons",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Salons_SalonId",
                table: "AspNetUsers",
                column: "SalonId",
                principalTable: "Salons",
                principalColumn: "salonId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Salons_AspNetUsers_OwnerId",
                table: "Salons",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Salons_SalonId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Salons_AspNetUsers_OwnerId",
                table: "Salons");

            migrationBuilder.DropIndex(
                name: "IX_Salons_OwnerId",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "city",
                table: "Salons");

            migrationBuilder.RenameColumn(
                name: "SalonId",
                table: "AspNetUsers",
                newName: "salonId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_SalonId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_salonId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Salons_salonId",
                table: "AspNetUsers",
                column: "salonId",
                principalTable: "Salons",
                principalColumn: "salonId");
        }
    }
}
