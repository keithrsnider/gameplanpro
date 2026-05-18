using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "drill_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_drill_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "practice_plans",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    intended_duration = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    key = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_practice_plans", x => x.id);
                    table.ForeignKey(
                        name: "fk_practice_plans_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "drills",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    instructions = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    demo_link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    drill_type_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    key = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_drills", x => x.id);
                    table.ForeignKey(
                        name: "fk_drills_drill_types_drill_type_id",
                        column: x => x.drill_type_id,
                        principalTable: "drill_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_drills_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sections",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    practice_plan_id = table.Column<int>(type: "integer", nullable: false),
                    key = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sections", x => x.id);
                    table.ForeignKey(
                        name: "fk_sections_practice_plans_practice_plan_id",
                        column: x => x.practice_plan_id,
                        principalTable: "practice_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_drills",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    instructions = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    demo_link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    coach_assignment = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    player_count = table.Column<int>(type: "integer", nullable: true),
                    station_group = table.Column<Guid>(type: "uuid", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    section_id = table.Column<int>(type: "integer", nullable: false),
                    drill_type_id = table.Column<int>(type: "integer", nullable: true),
                    drill_id = table.Column<int>(type: "integer", nullable: true),
                    key = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_drills", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_drills_drill_types_drill_type_id",
                        column: x => x.drill_type_id,
                        principalTable: "drill_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_plan_drills_drills_drill_id",
                        column: x => x.drill_id,
                        principalTable: "drills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_plan_drills_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "drill_types",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Hitting" },
                    { 2, "Pitching" },
                    { 3, "Base Running" },
                    { 4, "Fielding" },
                    { 5, "Conditioning" },
                    { 6, "Warm-up" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_drills_drill_type_id",
                table: "drills",
                column: "drill_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_drills_key",
                table: "drills",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_drills_user_id",
                table: "drills",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_drills_drill_id",
                table: "plan_drills",
                column: "drill_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_drills_drill_type_id",
                table: "plan_drills",
                column: "drill_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_drills_key",
                table: "plan_drills",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_drills_section_id",
                table: "plan_drills",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "ix_practice_plans_key",
                table: "practice_plans",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_practice_plans_user_id",
                table: "practice_plans",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sections_key",
                table: "sections",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sections_practice_plan_id",
                table: "sections",
                column: "practice_plan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plan_drills");

            migrationBuilder.DropTable(
                name: "drills");

            migrationBuilder.DropTable(
                name: "sections");

            migrationBuilder.DropTable(
                name: "drill_types");

            migrationBuilder.DropTable(
                name: "practice_plans");
        }
    }
}
