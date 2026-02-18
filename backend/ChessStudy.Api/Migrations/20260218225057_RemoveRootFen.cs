using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessStudy.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRootFen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RootFen",
                table: "ChessFiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RootFen",
                table: "ChessFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
