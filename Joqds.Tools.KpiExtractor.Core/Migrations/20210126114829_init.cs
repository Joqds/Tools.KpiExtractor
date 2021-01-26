using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Joqds.Tools.KpiExtractor.Core.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KpiTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatePart = table.Column<int>(type: "int", nullable: false),
                    ExtractionType = table.Column<int>(type: "int", nullable: false),
                    CleanUpAfter = table.Column<int>(type: "int", nullable: true),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiId = table.Column<int>(type: "int", nullable: false),
                    DbType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    IsNullable = table.Column<bool>(type: "bit", nullable: false),
                    ParameterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceColumn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceColumnNullMapping = table.Column<bool>(type: "bit", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Scale = table.Column<byte>(type: "tinyint", nullable: false),
                    Precision = table.Column<byte>(type: "tinyint", nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiParameters_KpiTypes_KpiId",
                        column: x => x.KpiId,
                        principalTable: "KpiTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KpiValues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KpiTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateTimePoint = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtractedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiValues_KpiTypes_KpiTypeId",
                        column: x => x.KpiTypeId,
                        principalTable: "KpiTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KpiParameters_KpiId",
                table: "KpiParameters",
                column: "KpiId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiValues_KpiTypeId",
                table: "KpiValues",
                column: "KpiTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KpiParameters");

            migrationBuilder.DropTable(
                name: "KpiValues");

            migrationBuilder.DropTable(
                name: "KpiTypes");
        }
    }
}
