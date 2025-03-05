﻿// <auto-generated />
using System;
using Gentlemen.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Gentlemen.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250304220648_UpdateOutfitSlugNullable3")]
    partial class UpdateOutfitSlugNullable3
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Gentlemen.Models.Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("PublishDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<int>("ViewCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Blogs", (string)null);
                });

            modelBuilder.Entity("Gentlemen.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CreatedAt = new DateTime(2025, 3, 5, 1, 6, 47, 833, DateTimeKind.Local).AddTicks(9420),
                            Description = "Profesyonel ve şık görünüm için öneriler",
                            ImageUrl = "/images/business-style.jpg",
                            IsActive = true,
                            Slug = "is-stili",
                            Title = "İş Stili"
                        },
                        new
                        {
                            Id = 2,
                            CreatedAt = new DateTime(2025, 3, 5, 1, 6, 47, 833, DateTimeKind.Local).AddTicks(9429),
                            Description = "Rahat ve trend günlük kombinler",
                            ImageUrl = "/images/casual-style.jpg",
                            IsActive = true,
                            Slug = "gunluk-stil",
                            Title = "Günlük Stil"
                        },
                        new
                        {
                            Id = 3,
                            CreatedAt = new DateTime(2025, 3, 5, 1, 6, 47, 833, DateTimeKind.Local).AddTicks(9430),
                            Description = "Özel anlar için şık seçimler",
                            ImageUrl = "/images/special-occasions.jpg",
                            IsActive = true,
                            Slug = "ozel-gunler",
                            Title = "Özel Günler"
                        });
                });

            modelBuilder.Entity("Gentlemen.Models.Outfit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClothingItems")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<decimal>("EstimatedPrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsFeatured")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Likes")
                        .HasColumnType("int");

                    b.Property<string>("Occasion")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Season")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Slug")
                        .HasColumnType("longtext");

                    b.Property<string>("Style")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Outfits", (string)null);
                });

            modelBuilder.Entity("Gentlemen.Models.StyleTip", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("longtext");

                    b.Property<bool>("IsFeatured")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Likes")
                        .HasColumnType("int");

                    b.Property<DateTime>("PublishDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("StyleTips", (string)null);
                });

            modelBuilder.Entity("Gentlemen.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<DateTime>("RegisterDate")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("Gentlemen.Models.StyleTip", b =>
                {
                    b.HasOne("Gentlemen.Models.Category", "CategoryObject")
                        .WithMany("StyleTips")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("CategoryObject");
                });

            modelBuilder.Entity("Gentlemen.Models.Category", b =>
                {
                    b.Navigation("StyleTips");
                });
#pragma warning restore 612, 618
        }
    }
}
