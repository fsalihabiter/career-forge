using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerForge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReviewItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewItems",
                schema: "careerforge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewItems_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "careerforge",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewItems_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "careerforge",
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewItems_QuestionId",
                schema: "careerforge",
                table: "ReviewItems",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewItems_UserId_QuestionId",
                schema: "careerforge",
                table: "ReviewItems",
                columns: new[] { "UserId", "QuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewItems",
                schema: "careerforge");
        }
    }
}
