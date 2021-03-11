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
    [Migration("20210307172323_guild id on reminder")]
    partial class guildidonreminder
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Espeon.GuildPrefixes", b =>
                {
                    b.Property<decimal>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string[]>("Values")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("prefixes");

                    b.HasKey("GuildId");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("prefixes");
                });

            modelBuilder.Entity("Espeon.GuildTags", b =>
                {
                    b.Property<decimal>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.HasKey("GuildId");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("guild_tags");
                });

            modelBuilder.Entity("Espeon.Tag", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<long>("CreateAt")
                        .HasColumnType("bigint")
                        .HasColumnName("created_at");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("tag_key");

                    b.Property<int>("Uses")
                        .HasColumnType("integer")
                        .HasColumnName("uses");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("tag_string");

                    b.HasKey("Id");

                    b.ToTable("tags");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Tag");
                });

            modelBuilder.Entity("Espeon.UserLocalisation", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<int>("Value")
                        .HasColumnType("integer")
                        .HasColumnName("localisation");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("localisation");
                });

            modelBuilder.Entity("Espeon.UserReminder", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("ReminderMessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.Property<long>("TriggerAt")
                        .HasColumnType("bigint")
                        .HasColumnName("trigger_at");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("reminder_string");

                    b.HasKey("Id");

                    b.ToTable("reminders");
                });

            modelBuilder.Entity("Espeon.GlobalTag", b =>
                {
                    b.HasBaseType("Espeon.Tag");

                    b.ToTable("tags");

                    b.HasDiscriminator().HasValue("GlobalTag");
                });

            modelBuilder.Entity("Espeon.GuildTag", b =>
                {
                    b.HasBaseType("Espeon.Tag");

                    b.Property<decimal>("CreatorId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("creator_id");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("OwnerId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("owner_id");

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

                    b.Navigation("GuildTags");
                });

            modelBuilder.Entity("Espeon.GuildTags", b =>
                {
                    b.Navigation("Values");
                });
#pragma warning restore 612, 618
        }
    }
}