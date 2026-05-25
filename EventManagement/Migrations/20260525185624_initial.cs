using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagement.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    total_seats = table.Column<int>(type: "integer", nullable: false),
                    available_seats = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                    table.CheckConstraint("chk_events_available_seats", "available_seats <= total_seats");
                    table.CheckConstraint("chk_events_end_at", "end_at >= start_at");
                    table.CheckConstraint("chk_events_title", "LENGTH(title) > 0");
                    table.CheckConstraint("chk_events_total_seats", "total_seats > 0");
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    seats_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processing_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessingAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.id);
                    table.CheckConstraint("chk_bookings_seats_count", "seats_count > 0");
                    table.CheckConstraint("chk_bookings_status", "status IN('Pending', 'Confirmed', 'Rejected', 'Processing')");
                    table.ForeignKey(
                        name: "FK_bookings_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_event_id",
                table: "bookings",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_events_end_at",
                table: "events",
                column: "end_at");

            migrationBuilder.CreateIndex(
                name: "IX_events_start_at",
                table: "events",
                column: "start_at");

            migrationBuilder.CreateIndex(
                name: "IX_events_title",
                table: "events",
                column: "title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "events");
        }
    }
}
