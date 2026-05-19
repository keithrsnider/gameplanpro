using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachAndPlayerCountToDrill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "coach_id",
                table: "drills",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "number_of_players",
                table: "drills",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_drills_coach_id",
                table: "drills",
                column: "coach_id");

            migrationBuilder.AddForeignKey(
                name: "fk_drills_coaches_coach_id",
                table: "drills",
                column: "coach_id",
                principalTable: "coaches",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_drills_coaches_coach_id",
                table: "drills");

            migrationBuilder.DropIndex(
                name: "ix_drills_coach_id",
                table: "drills");

            migrationBuilder.DropColumn(
                name: "coach_id",
                table: "drills");

            migrationBuilder.DropColumn(
                name: "number_of_players",
                table: "drills");
        }
    }
}
