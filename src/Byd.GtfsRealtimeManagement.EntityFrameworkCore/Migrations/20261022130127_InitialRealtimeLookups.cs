using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Byd.GtfsRealtimeManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialRealtimeLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gtfs_realtime_management");

            migrationBuilder.CreateTable(
                name: "StopScheduleRelationships",
                schema: "gtfs_realtime_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stop_schedule_relationships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TripScheduleRelationships",
                schema: "gtfs_realtime_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_schedule_relationships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleStopStatuses",
                schema: "gtfs_realtime_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_stop_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ViolationTypes",
                schema: "gtfs_realtime_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_violation_types", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StopScheduleRelationships_Code",
                schema: "gtfs_realtime_management",
                table: "StopScheduleRelationships",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripScheduleRelationships_Code",
                schema: "gtfs_realtime_management",
                table: "TripScheduleRelationships",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleStopStatuses_Code",
                schema: "gtfs_realtime_management",
                table: "VehicleStopStatuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViolationTypes_Code",
                schema: "gtfs_realtime_management",
                table: "ViolationTypes",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StopScheduleRelationships",
                schema: "gtfs_realtime_management");

            migrationBuilder.DropTable(
                name: "TripScheduleRelationships",
                schema: "gtfs_realtime_management");

            migrationBuilder.DropTable(
                name: "VehicleStopStatuses",
                schema: "gtfs_realtime_management");

            migrationBuilder.DropTable(
                name: "ViolationTypes",
                schema: "gtfs_realtime_management");
        }
    }
}
