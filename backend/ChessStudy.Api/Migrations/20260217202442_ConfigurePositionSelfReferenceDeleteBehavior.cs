using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessStudy.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurePositionSelfReferenceDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Positions_ParentPositionId",
                table: "Positions");

            migrationBuilder.CreateTable(
                name: "Arrows",
                columns: table => new
                {
                    ArrowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    FromSquare = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    ToSquare = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrows", x => x.ArrowId);
                    table.ForeignKey(
                        name: "FK_Arrows_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Arrows_PositionId",
                table: "Arrows",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Positions_ParentPositionId",
                table: "Positions",
                column: "ParentPositionId",
                principalTable: "Positions",
                principalColumn: "PositionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Positions_ParentPositionId",
                table: "Positions");

            migrationBuilder.DropTable(
                name: "Arrows");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Positions_ParentPositionId",
                table: "Positions",
                column: "ParentPositionId",
                principalTable: "Positions",
                principalColumn: "PositionId");
        }
    }
}
