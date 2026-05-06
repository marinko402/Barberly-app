using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Salons",
                columns: table => new
                {
                    salonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salons", x => x.salonId);
                });

            migrationBuilder.CreateTable(
                name: "Barbers",
                columns: table => new
                {
                    barberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    salonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barbers", x => x.barberId);
                    table.ForeignKey(
                        name: "FK_Barbers_Salons_salonId",
                        column: x => x.salonId,
                        principalTable: "Salons",
                        principalColumn: "salonId");
                });

            migrationBuilder.CreateTable(
                name: "Timeslots",
                columns: table => new
                {
                    timeslotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    startTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    duration = table.Column<int>(type: "int", nullable: false),
                    salonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    barberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timeslots", x => x.timeslotId);
                    table.ForeignKey(
                        name: "FK_Timeslots_Barbers_barberId",
                        column: x => x.barberId,
                        principalTable: "Barbers",
                        principalColumn: "barberId");
                    table.ForeignKey(
                        name: "FK_Timeslots_Salons_salonId",
                        column: x => x.salonId,
                        principalTable: "Salons",
                        principalColumn: "salonId");
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    bookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    timeslotId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    customerFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customerLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customerPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.bookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_Timeslots_timeslotId",
                        column: x => x.timeslotId,
                        principalTable: "Timeslots",
                        principalColumn: "timeslotId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Barbers_salonId",
                table: "Barbers",
                column: "salonId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_timeslotId",
                table: "Bookings",
                column: "timeslotId");

            migrationBuilder.CreateIndex(
                name: "IX_Timeslots_barberId",
                table: "Timeslots",
                column: "barberId");

            migrationBuilder.CreateIndex(
                name: "IX_Timeslots_salonId",
                table: "Timeslots",
                column: "salonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Timeslots");

            migrationBuilder.DropTable(
                name: "Barbers");

            migrationBuilder.DropTable(
                name: "Salons");
        }
    }
}
