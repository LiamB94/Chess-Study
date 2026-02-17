using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessStudy.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChessFileId = table.Column<int>(type: "int", nullable: false),
                    ParentPositionId = table.Column<int>(type: "int", nullable: true),
                    MoveUci = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    MoveSan = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Fen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ply = table.Column<int>(type: "int", nullable: false),
                    SiblingOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_Positions_ChessFiles_ChessFileId",
                        column: x => x.ChessFileId,
                        principalTable: "ChessFiles",
                        principalColumn: "ChessFileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Positions_Positions_ParentPositionId",
                        column: x => x.ParentPositionId,
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ChessFileId",
                table: "Positions",
                column: "ChessFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ParentPositionId",
                table: "Positions",
                column: "ParentPositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Positions");
        }
    }
}
