using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byd.GtfsRealtimeManagement.Migrations
{
    /// <inheritdoc />
    public partial class CreateRealtimeStopTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RealtimeStopTimes",
                schema: "gtfs_realtime_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    realtime_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_schedule_relationship_id = table.Column<int>(type: "integer", nullable: false),
                    stop_schedule_relationship_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_stop_status_id = table.Column<int>(type: "integer", nullable: true),
                    violation_type_id = table.Column<int>(type: "integer", nullable: true),
                    gtfs_feed_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<string>(type: "text", nullable: true),
                    gtfs_trip_id = table.Column<string>(type: "text", nullable: false),
                    gtfs_route_id = table.Column<string>(type: "text", nullable: false),
                    stop_sequence = table.Column<int>(type: "integer", nullable: false),
                    gtfs_stop_id = table.Column<string>(type: "text", nullable: false),
                    arrival_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    reported_delay = table.Column<int>(type: "integer", nullable: true),
                    arrival_correlation_id = table.Column<string>(type: "text", nullable: true),
                    departure_correlation_id = table.Column<string>(type: "text", nullable: true),
                    status_correlation_id = table.Column<string>(type: "text", nullable: true),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_realtime_stop_times", x => x.id);
                    table.ForeignKey(
                        name: "fk_realtime_stop_times_realtime_sources_realtime_source_id",
                        column: x => x.realtime_source_id,
                        principalSchema: "gtfs_realtime_management",
                        principalTable: "RealtimeSources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_realtime_stop_times_stop_schedule_relationships_stop_schedule_r",
                        column: x => x.stop_schedule_relationship_id,
                        principalSchema: "gtfs_realtime_management",
                        principalTable: "StopScheduleRelationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_realtime_stop_times_trip_schedule_relationships_trip_schedule_r",
                        column: x => x.trip_schedule_relationship_id,
                        principalSchema: "gtfs_realtime_management",
                        principalTable: "TripScheduleRelationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_realtime_stop_times_vehicle_stop_statuses_vehicle_stop_status_id",
                        column: x => x.vehicle_stop_status_id,
                        principalSchema: "gtfs_realtime_management",
                        principalTable: "VehicleStopStatuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_realtime_stop_times_violation_types_violation_type_id",
                        column: x => x.violation_type_id,
                        principalSchema: "gtfs_realtime_management",
                        principalTable: "ViolationTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RealtimeStopTimes_Source_Service_Route",
                schema: "gtfs_realtime_management",
                table: "RealtimeStopTimes",
                columns: new[] { "realtime_source_id", "service_date", "gtfs_route_id" });

            migrationBuilder.CreateIndex(
                name: "IX_RealtimeStopTimes_Source_Service_Trip_Start_Sequence",
                schema: "gtfs_realtime_management",
                table: "RealtimeStopTimes",
                columns: new[] { "realtime_source_id", "service_date", "gtfs_trip_id", "start_time", "stop_sequence" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_RealtimeStopTimes_Source_Service_Vehicle",
                schema: "gtfs_realtime_management",
                table: "RealtimeStopTimes",
                columns: new[] { "realtime_source_id", "service_date", "vehicle_id" });

            migrationBuilder.CreateIndex(
                name: "IX_RealtimeStopTimes_Source_Service_Violation",
                schema: "gtfs_realtime_management",
                table: "RealtimeStopTimes",
                columns: new[] { "realtime_source_id", "service_date", "violation_type_id" });

            migrationBuilder.CreateIndex(
                name: "ix_realtime_stop_times_stop_schedule_relationship_id",
                schema: "gtfs_realtime_management",
                table: "RealtimeStopTimes",
                column: "stop_schedule_relationship_id");

            migrationBuilder.CreateIndex(
                name: "ix_realtime_stop_times_trip_schedule_relationship_id",
                schema: "gtfs_realtime_management",
                table: "RealtimeStopTimes",
                column: "trip_schedule_relationship_id");

            migrationBuilder.CreateIndex(
                name: "ix_realtime_stop_times_vehicle_stop_status_id",
                schema: "gtfs_realtime_management",
                table: "RealtimeStopTimes",
                column: "vehicle_stop_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_realtime_stop_times_violation_type_id",
                schema: "gtfs_realtime_management",
                table: "RealtimeStopTimes",
                column: "violation_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RealtimeStopTimes",
                schema: "gtfs_realtime_management");
        }
    }
}
