using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byd.GtfsRealtimeManagement.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleActiveRealtimeSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RealtimeSources_Active_Organization",
                schema: "gtfs_realtime_management",
                table: "RealtimeSources",
                columns: new[] { "tenant_id", "organization_id" },
                unique: true,
                filter: "is_active = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RealtimeSources_Active_Organization",
                schema: "gtfs_realtime_management",
                table: "RealtimeSources");
        }
    }
}
