using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarPark.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParkingSpaces",
                columns: table => new
                {
                    SpaceNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpaces", x => x.SpaceNumber);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleReg = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TimeIn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    SpaceNumber = table.Column<int>(type: "int", nullable: false),
                    TimeOut = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    VehicleCharge = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_ParkingSpaces_SpaceNumber",
                        column: x => x.SpaceNumber,
                        principalTable: "ParkingSpaces",
                        principalColumn: "SpaceNumber",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ParkingSpaces",
                column: "SpaceNumber",
                values: new object[]
                {
                    1,
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    8,
                    9,
                    10
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_SpaceNumber",
                table: "ParkingSessions",
                column: "SpaceNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingSessions");

            migrationBuilder.DropTable(
                name: "ParkingSpaces");
        }
    }
}
