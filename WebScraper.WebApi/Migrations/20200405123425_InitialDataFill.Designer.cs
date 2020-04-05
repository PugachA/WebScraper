﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebScraper.WebApi.Models;

namespace WebScraper.WebApi.Migrations
{
    [DbContext(typeof(ProductWatcherContext))]
    [Migration("20200405123425_InitialDataFill")]
    partial class InitialDataFill
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("WebScraper.WebApi.DTO.PriceDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DicountPrice")
                        .HasColumnType("int");

                    b.Property<double?>("DiscountPercentage")
                        .HasColumnType("float");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.ToTable("Prices");
                });

            modelBuilder.Entity("WebScraper.WebApi.DTO.ProductDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Scheduler")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SiteId")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SiteId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("WebScraper.WebApi.DTO.SiteDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<int?>("SettingsId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SettingsId");

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("WebScraper.WebApi.DTO.SiteSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("AutoGenerateSchedule")
                        .HasColumnType("bit");

                    b.Property<string>("BaseUrl")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("CheckInterval")
                        .IsRequired()
                        .HasColumnType("nvarchar(48)");

                    b.Property<string>("MinCheckInterval")
                        .IsRequired()
                        .HasColumnType("nvarchar(48)");

                    b.HasKey("Id");

                    b.ToTable("SiteSettings");
                });

            modelBuilder.Entity("WebScraper.WebApi.DTO.PriceDto", b =>
                {
                    b.HasOne("WebScraper.WebApi.DTO.ProductDto", "Product")
                        .WithMany("Prices")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebScraper.WebApi.DTO.ProductDto", b =>
                {
                    b.HasOne("WebScraper.WebApi.DTO.SiteDto", "Site")
                        .WithMany("Products")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebScraper.WebApi.DTO.SiteDto", b =>
                {
                    b.HasOne("WebScraper.WebApi.DTO.SiteSettings", "Settings")
                        .WithMany()
                        .HasForeignKey("SettingsId");
                });
#pragma warning restore 612, 618
        }
    }
}
