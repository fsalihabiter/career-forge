using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerForge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class PersistRubricEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvaluationJson",
                schema: "careerforge",
                table: "SessionQuestions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvaluationJson",
                schema: "careerforge",
                table: "SessionQuestions");
        }
    }
}
