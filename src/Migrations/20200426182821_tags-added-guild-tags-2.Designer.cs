﻿// <auto-generated />
using System;
using Espeon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Espeon.Migrations
{
    [DbContext(typeof(EspeonDbContext))]
    [Migration("20200426182821_tags-added-guild-tags-2")]
    partial class tagsaddedguildtags2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "5.0.0-preview.3.20181.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Espeon.GuildPrefixes", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnName("guild_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string[]>("Values")
                        .IsRequired()
                        .HasColumnName("prefixes")
                        .HasColumnType("text[]");

                    b.HasKey("GuildId");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("prefixes");
                });

            modelBuilder.Entity("Espeon.GuildTags", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnName("guild_id")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("GuildId");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("guild_tags");
                });

            modelBuilder.Entity("Espeon.Tag", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("text");

                    b.Property<long>("CreateAt")
                        .HasColumnName("created_at")
                        .HasColumnType("bigint");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnName("tag_key")
                        .HasColumnType("text");

                    b.Property<int>("Uses")
                        .HasColumnName("uses")
                        .HasColumnType("integer");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnName("tag_string")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("tags");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Tag");
                });

            modelBuilder.Entity("Espeon.UserLocalisation", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnName("guild_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Value")
                        .HasColumnName("localisation")
                        .HasColumnType("integer");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("localisation");
                });

            modelBuilder.Entity("Espeon.UserReminder", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("text");

                    b.Property<decimal>("ChannelId")
                        .HasColumnName("channel_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ReminderMessageId")
                        .HasColumnName("message_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<long>("TriggerAt")
                        .HasColumnName("trigger_at")
                        .HasColumnType("bigint");

                    b.Property<decimal>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnName("reminder_string")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("reminders");
                });

            modelBuilder.Entity("Espeon.GuildTag", b =>
                {
                    b.HasBaseType("Espeon.Tag");

                    b.Property<decimal>("CreatorId")
                        .HasColumnName("creator_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnName("guild_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("OwnerId")
                        .HasColumnName("owner_id")
                        .HasColumnType("numeric(20,0)");

                    b.HasIndex("GuildId");

                    b.ToTable("tags");

                    b.HasDiscriminator().HasValue("GuildTag");
                });

            modelBuilder.Entity("Espeon.GuildTag", b =>
                {
                    b.HasOne("Espeon.GuildTags", "GuildTags")
                        .WithMany("Values")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
