using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuLazerServer.Migrations
{
    public partial class UpdateUserFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PerfomancePoints",
                table: "stats_taiko",
                newName: "PerformancePoints");

            migrationBuilder.RenameColumn(
                name: "PerfomancePoints",
                table: "stats_osu",
                newName: "PerformancePoints");

            migrationBuilder.RenameColumn(
                name: "PerfomancePoints",
                table: "stats_mania",
                newName: "PerformancePoints");

            migrationBuilder.RenameColumn(
                name: "PerfomancePoints",
                table: "stats_fruits",
                newName: "PerformancePoints");
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.RenameColumn(
                name: "PerformancePoints",
                table: "stats_taiko",
                newName: "PerfomancePoints");

            migrationBuilder.RenameColumn(
                name: "PerformancePoints",
                table: "stats_osu",
                newName: "PerfomancePoints");

            migrationBuilder.RenameColumn(
                name: "PerformancePoints",
                table: "stats_mania",
                newName: "PerfomancePoints");

            migrationBuilder.RenameColumn(
                name: "PerformancePoints",
                table: "stats_fruits",
                newName: "PerfomancePoints");
        }
    }
}
