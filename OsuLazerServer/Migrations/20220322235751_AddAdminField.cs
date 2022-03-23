using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsuLazerServer.Migrations
{
    public partial class AddAdminField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>("is_admin", "users", nullable: false, defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("is_admin", "users");
        }
    }
}
