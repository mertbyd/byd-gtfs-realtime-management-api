using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byd.GtfsRealtimeManagement.Migrations
{
    /// <inheritdoc />
    public partial class CreateRealtimeSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RealtimeSources",
                schema: "gtfs_realtime_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    vehicle_positions_url = table.Column<string>(type: "text", nullable: true),
                    trip_updates_url = table.Column<string>(type: "text", nullable: true),
                    service_alerts_url = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_realtime_sources", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RealtimeSources_Scope_Name",
                schema: "gtfs_realtime_management",
                table: "RealtimeSources",
                columns: new[] { "tenant_id", "organization_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RealtimeSources",
                schema: "gtfs_realtime_management");
        }
    }
}
