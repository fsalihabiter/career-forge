using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerForge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SkillAssessmentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkillAssessments",
                schema: "careerforge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserSkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RollingScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MeasuredLevel = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EvidenceCount = table.Column<int>(type: "integer", nullable: false),
                    TotalEvidenceCount = table.Column<int>(type: "integer", nullable: false),
                    AssessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillAssessments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "careerforge",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillAssessments_InterviewSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "careerforge",
                        principalTable: "InterviewSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillAssessments_UserSkills_UserSkillId",
                        column: x => x.UserSkillId,
                        principalSchema: "careerforge",
                        principalTable: "UserSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillAssessments_SessionId_UserSkillId",
                schema: "careerforge",
                table: "SkillAssessments",
                columns: new[] { "SessionId", "UserSkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillAssessments_UserId",
                schema: "careerforge",
                table: "SkillAssessments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillAssessments_UserSkillId",
                schema: "careerforge",
                table: "SkillAssessments",
                column: "UserSkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkillAssessments",
                schema: "careerforge");
        }
    }
}
