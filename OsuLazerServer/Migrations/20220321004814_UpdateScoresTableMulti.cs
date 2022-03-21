using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuLazerServer.Migrations
{
    public partial class UpdateScoresTableMulti : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>("submittion_playlist", "scores", nullable: true);
            migrationBuilder.AddColumn<int>("submitted_in", "scores", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("submittion_playlist", "scores");
            migrationBuilder.DropColumn("submitted_in", "scores");
        }
    }
}
