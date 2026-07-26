using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerForge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReviewScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EaseFactor",
                schema: "careerforge",
                table: "ReviewItems",
                type: "numeric",
                nullable: false,
                defaultValue: 2.5m);

            migrationBuilder.AddColumn<int>(
                name: "IntervalDays",
                schema: "careerforge",
                table: "ReviewItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastReviewedAt",
                schema: "careerforge",
                table: "ReviewItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextReviewAt",
                schema: "careerforge",
                table: "ReviewItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "RepetitionCount",
                schema: "careerforge",
                table: "ReviewItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE careerforge."ReviewItems"
                SET "NextReviewAt" = "AddedAt"
                WHERE "NextReviewAt" < TIMESTAMPTZ '2000-01-01';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EaseFactor",
                schema: "careerforge",
                table: "ReviewItems");

            migrationBuilder.DropColumn(
                name: "IntervalDays",
                schema: "careerforge",
                table: "ReviewItems");

            migrationBuilder.DropColumn(
                name: "LastReviewedAt",
                schema: "careerforge",
                table: "ReviewItems");

            migrationBuilder.DropColumn(
                name: "NextReviewAt",
                schema: "careerforge",
                table: "ReviewItems");

            migrationBuilder.DropColumn(
                name: "RepetitionCount",
                schema: "careerforge",
                table: "ReviewItems");
        }
    }
}
