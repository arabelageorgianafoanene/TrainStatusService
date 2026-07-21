using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainStatusWorker.Migrations
{
    /// <inheritdoc />
    public partial class InitialReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainSummaries",
                columns: table => new
                {
                    TrainId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStatus = table.Column<string>(type: "text", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainSummaries", x => x.TrainId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainSummaries");
        }
    }
}
