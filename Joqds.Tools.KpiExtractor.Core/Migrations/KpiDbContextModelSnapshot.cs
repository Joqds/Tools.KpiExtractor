﻿// <auto-generated />
using System;
using Joqds.Tools.KpiExtractor.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Joqds.Tools.KpiExtractor.Core.Migrations
{
    [DbContext(typeof(KpiDbContext))]
    partial class KpiDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Joqds.Tools.KpiExtractor.Contract.KpiParameter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("DbType")
                        .HasColumnType("int");

                    b.Property<string>("DefaultValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Direction")
                        .HasColumnType("int");

                    b.Property<bool>("IsNullable")
                        .HasColumnType("bit");

                    b.Property<int>("KpiId")
                        .HasColumnType("int");

                    b.Property<string>("ParameterName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte>("Precision")
                        .HasColumnType("tinyint");

                    b.Property<byte>("Scale")
                        .HasColumnType("tinyint");

                    b.Property<int>("Size")
                        .HasColumnType("int");

                    b.Property<string>("SourceColumn")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("SourceColumnNullMapping")
                        .HasColumnType("bit");

                    b.Property<string>("SystemName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("KpiId");

                    b.ToTable("KpiParameters");
                });

            modelBuilder.Entity("Joqds.Tools.KpiExtractor.Contract.KpiType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int?>("CleanUpAfter")
                        .HasColumnType("int");

                    b.Property<int>("DatePart")
                        .HasColumnType("int");

                    b.Property<int>("ExtractionType")
                        .HasColumnType("int");

                    b.Property<string>("Query")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("KpiTypes");
                });

            modelBuilder.Entity("Joqds.Tools.KpiExtractor.Contract.KpiValue", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<DateTime>("DateTimePoint")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ExtractedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("KpiTypeId")
                        .HasColumnType("int");

                    b.Property<string>("TargetId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Value")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("KpiTypeId");

                    b.ToTable("KpiValues");
                });

            modelBuilder.Entity("Joqds.Tools.KpiExtractor.Contract.KpiParameter", b =>
                {
                    b.HasOne("Joqds.Tools.KpiExtractor.Contract.KpiType", "Kpi")
                        .WithMany("Parameters")
                        .HasForeignKey("KpiId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Kpi");
                });

            modelBuilder.Entity("Joqds.Tools.KpiExtractor.Contract.KpiValue", b =>
                {
                    b.HasOne("Joqds.Tools.KpiExtractor.Contract.KpiType", "KpiType")
                        .WithMany("KpiValues")
                        .HasForeignKey("KpiTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("KpiType");
                });

            modelBuilder.Entity("Joqds.Tools.KpiExtractor.Contract.KpiType", b =>
                {
                    b.Navigation("KpiValues");

                    b.Navigation("Parameters");
                });
#pragma warning restore 612, 618
        }
    }
}
