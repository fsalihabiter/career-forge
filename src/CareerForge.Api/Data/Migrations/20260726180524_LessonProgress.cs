using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerForge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class LessonProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonProgress",
                schema: "careerforge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonStableId = table.Column<string>(type: "text", nullable: false),
                    LessonVersion = table.Column<int>(type: "integer", nullable: false),
                    LastSectionKey = table.Column<string>(type: "text", nullable: false),
                    CompletedSectionKeysJson = table.Column<string>(type: "jsonb", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "careerforge",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgress_UserId_LessonStableId_LessonVersion",
                schema: "careerforge",
                table: "LessonProgress",
                columns: new[] { "UserId", "LessonStableId", "LessonVersion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonProgress",
                schema: "careerforge");
        }
    }
}
