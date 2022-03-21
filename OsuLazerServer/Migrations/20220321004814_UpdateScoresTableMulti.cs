using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuLazerServer.Migrations
{
    public partial class UpdateScoresTableMulti : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>("submittion_playlist", "scores", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>("submitted_in", "scores", nullable: false, defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("submittion_playlist", "scores");
            migrationBuilder.DropColumn("submitted_in", "scores");
        }
    }
}
