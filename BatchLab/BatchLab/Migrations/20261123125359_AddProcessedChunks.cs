using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BatchLab.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedChunks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedChunks",
                columns: table => new
                {
                    OperationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    Signature = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedChunks", x => new { x.OperationId, x.ChunkIndex });
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedChunks");
        }
    }
}
