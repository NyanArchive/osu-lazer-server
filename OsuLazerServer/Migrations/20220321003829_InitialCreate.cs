using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OsuLazerServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    allowed_write = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scores",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    beatmap_id = table.Column<int>(type: "integer", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    total_score = table.Column<long>(type: "bigint", nullable: false),
                    accuracy = table.Column<double>(type: "double precision", nullable: false),
                    perfomance_points = table.Column<double>(type: "double precision", nullable: true),
                    max_combo = table.Column<int>(type: "integer", nullable: false),
                    ruleset_id = table.Column<int>(type: "integer", nullable: false),
                    mods = table.Column<List<string>>(type: "text[]", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    statistics = table.Column<string>(type: "text", nullable: false),
                    submitted_in = table.Column<int>(type: "integer", nullable: false),
                    submittion_playlist = table.Column<int>(type: "integer", nullable: false),
                    passed = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stats_fruits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfomancePoints = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    LevelProgress = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<long>(type: "bigint", nullable: false),
                    TotalHits = table.Column<long>(type: "bigint", nullable: false),
                    MaxCombo = table.Column<int>(type: "integer", nullable: false),
                    RankedScore = table.Column<long>(type: "bigint", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stats_fruits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stats_mania",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfomancePoints = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    LevelProgress = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<long>(type: "bigint", nullable: false),
                    TotalHits = table.Column<long>(type: "bigint", nullable: false),
                    MaxCombo = table.Column<int>(type: "integer", nullable: false),
                    RankedScore = table.Column<long>(type: "bigint", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stats_mania", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stats_osu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfomancePoints = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    LevelProgress = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<long>(type: "bigint", nullable: false),
                    TotalHits = table.Column<long>(type: "bigint", nullable: false),
                    MaxCombo = table.Column<int>(type: "integer", nullable: false),
                    RankedScore = table.Column<long>(type: "bigint", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stats_osu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stats_taiko",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfomancePoints = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    LevelProgress = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<long>(type: "bigint", nullable: false),
                    TotalHits = table.Column<long>(type: "bigint", nullable: false),
                    MaxCombo = table.Column<int>(type: "integer", nullable: false),
                    RankedScore = table.Column<long>(type: "bigint", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stats_taiko", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    nickname_history = table.Column<string[]>(type: "text[]", nullable: false),
                    banned = table.Column<bool>(type: "boolean", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: false),
                    play_count = table.Column<int>(type: "integer", nullable: false),
                    replays_watched = table.Column<int>(type: "integer", nullable: false),
                    StatsOsuId = table.Column<int>(type: "integer", nullable: false),
                    StatsTaikoId = table.Column<int>(type: "integer", nullable: false),
                    StatsFruitsId = table.Column<int>(type: "integer", nullable: false),
                    StatsManiaId = table.Column<int>(type: "integer", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_stats_fruits_StatsFruitsId",
                        column: x => x.StatsFruitsId,
                        principalTable: "stats_fruits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_stats_mania_StatsManiaId",
                        column: x => x.StatsManiaId,
                        principalTable: "stats_mania",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_stats_osu_StatsOsuId",
                        column: x => x.StatsOsuId,
                        principalTable: "stats_osu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_stats_taiko_StatsTaikoId",
                        column: x => x.StatsTaikoId,
                        principalTable: "stats_taiko",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_StatsFruitsId",
                table: "users",
                column: "StatsFruitsId");

            migrationBuilder.CreateIndex(
                name: "IX_users_StatsManiaId",
                table: "users",
                column: "StatsManiaId");

            migrationBuilder.CreateIndex(
                name: "IX_users_StatsOsuId",
                table: "users",
                column: "StatsOsuId");

            migrationBuilder.CreateIndex(
                name: "IX_users_StatsTaikoId",
                table: "users",
                column: "StatsTaikoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "channels");

            migrationBuilder.DropTable(
                name: "scores");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "stats_fruits");

            migrationBuilder.DropTable(
                name: "stats_mania");

            migrationBuilder.DropTable(
                name: "stats_osu");

            migrationBuilder.DropTable(
                name: "stats_taiko");
        }
    }
}
