﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OsuLazerServer.Database;

#nullable disable

namespace OsuLazerServer.Migrations
{
    [DbContext(typeof(LazerContext))]
    [Migration("20220324120554_UpdateUserFields")]
    partial class UpdateUserFields
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-preview.1.22076.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OsuLazerServer.Database.Tables.Channel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("AllowedWrite")
                        .HasColumnType("boolean")
                        .HasColumnName("allowed_write");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.ToTable("channels");
                });

            modelBuilder.Entity("OsuLazerServer.Database.Tables.Scores.DbScore", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Accuracy")
                        .HasColumnType("double precision")
                        .HasColumnName("accuracy");

                    b.Property<int>("BeatmapId")
                        .HasColumnType("integer")
                        .HasColumnName("beatmap_id");

                    b.Property<int>("MaxCombo")
                        .HasColumnType("integer")
                        .HasColumnName("max_combo");

                    b.Property<List<string>>("Mods")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("mods");

                    b.Property<bool>("Passed")
                        .HasColumnType("boolean")
                        .HasColumnName("passed");

                    b.Property<double>("PerfomancePoints")
                        .HasColumnType("double precision")
                        .HasColumnName("perfomance_points");

                    b.Property<int>("Rank")
                        .HasColumnType("integer")
                        .HasColumnName("rank");

                    b.Property<int>("RuleSetId")
                        .HasColumnType("integer")
                        .HasColumnName("ruleset_id");

                    b.Property<string>("Statistics")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("statistics");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<DateTimeOffset>("SubmittedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("submitted_at");

                    b.Property<int>("SubmittedIn")
                        .HasColumnType("integer")
                        .HasColumnName("submitted_in");

                    b.Property<int>("SubmittionPlaylist")
                        .HasColumnType("integer")
                        .HasColumnName("submittion_playlist");

                    b.Property<long>("TotalScore")
                        .HasColumnType("bigint")
                        .HasColumnName("total_score");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.ToTable("scores");
                });

            modelBuilder.Entity("OsuLazerServer.Database.Tables.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Banned")
                        .HasColumnType("boolean")
                        .HasColumnName("banned");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("country");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("boolean")
                        .HasColumnName("is_admin");

                    b.Property<DateTime>("JoinedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("joined_at");

                    b.Property<string[]>("NicknameHistory")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("nickname_history");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password");

                    b.Property<int>("PlayCount")
                        .HasColumnType("integer")
                        .HasColumnName("play_count");

                    b.Property<int>("ReplaysWatches")
                        .HasColumnType("integer")
                        .HasColumnName("replays_watched");

                    b.Property<int>("StatsFruitsId")
                        .HasColumnType("integer");

                    b.Property<int>("StatsManiaId")
                        .HasColumnType("integer");

                    b.Property<int>("StatsOsuId")
                        .HasColumnType("integer");

                    b.Property<int>("StatsTaikoId")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.HasIndex("StatsFruitsId");

                    b.HasIndex("StatsManiaId");

                    b.HasIndex("StatsOsuId");

                    b.HasIndex("StatsTaikoId");

                    b.ToTable("users");
                });

            modelBuilder.Entity("OsuLazerServer.Database.Tables.UsersStatsFruits", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<float>("Accuracy")
                        .HasColumnType("real");

                    b.Property<int>("Level")
                        .HasColumnType("integer");

                    b.Property<int>("LevelProgress")
                        .HasColumnType("integer");

                    b.Property<int>("MaxCombo")
                        .HasColumnType("integer");

                    b.Property<int>("PerformancePoints")
                        .HasColumnType("integer");

                    b.Property<long>("RankedScore")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalHits")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalScore")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("stats_fruits");
                });

            modelBuilder.Entity("OsuLazerServer.Database.Tables.UsersStatsMania", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<float>("Accuracy")
                        .HasColumnType("real");

                    b.Property<int>("Level")
                        .HasColumnType("integer");

                    b.Property<int>("LevelProgress")
                        .HasColumnType("integer");

                    b.Property<int>("MaxCombo")
                        .HasColumnType("integer");

                    b.Property<int>("PerformancePoints")
                        .HasColumnType("integer");

                    b.Property<long>("RankedScore")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalHits")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalScore")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("stats_mania");
                });

            modelBuilder.Entity("OsuLazerServer.Database.Tables.UsersStatsOsu", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<float>("Accuracy")
                        .HasColumnType("real");

                    b.Property<int>("Level")
                        .HasColumnType("integer");

                    b.Property<int>("LevelProgress")
                        .HasColumnType("integer");

                    b.Property<int>("MaxCombo")
                        .HasColumnType("integer");

                    b.Property<int>("PerformancePoints")
                        .HasColumnType("integer");

                    b.Property<long>("RankedScore")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalHits")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalScore")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("stats_osu");
                });

            modelBuilder.Entity("OsuLazerServer.Database.Tables.UsersStatsTaiko", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<float>("Accuracy")
                        .HasColumnType("real");

                    b.Property<int>("Level")
                        .HasColumnType("integer");

                    b.Property<int>("LevelProgress")
                        .HasColumnType("integer");

                    b.Property<int>("MaxCombo")
                        .HasColumnType("integer");

                    b.Property<int>("PerformancePoints")
                        .HasColumnType("integer");

                    b.Property<long>("RankedScore")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalHits")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalScore")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("stats_taiko");
                });

            modelBuilder.Entity("OsuLazerServer.Database.Tables.User", b =>
                {
                    b.HasOne("OsuLazerServer.Database.Tables.UsersStatsFruits", "StatsFruits")
                        .WithMany()
                        .HasForeignKey("StatsFruitsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OsuLazerServer.Database.Tables.UsersStatsMania", "StatsMania")
                        .WithMany()
                        .HasForeignKey("StatsManiaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OsuLazerServer.Database.Tables.UsersStatsOsu", "StatsOsu")
                        .WithMany()
                        .HasForeignKey("StatsOsuId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OsuLazerServer.Database.Tables.UsersStatsTaiko", "StatsTaiko")
                        .WithMany()
                        .HasForeignKey("StatsTaikoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StatsFruits");

                    b.Navigation("StatsMania");

                    b.Navigation("StatsOsu");

                    b.Navigation("StatsTaiko");
                });
#pragma warning restore 612, 618
        }
    }
}
