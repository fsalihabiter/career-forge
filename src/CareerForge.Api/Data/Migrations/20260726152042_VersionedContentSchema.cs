using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerForge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class VersionedContentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishedAt",
                schema: "careerforge",
                table: "Questions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RubricId",
                schema: "careerforge",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "careerforge",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateTable(
                name: "Rubrics",
                schema: "careerforge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StableId = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rubrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VersionedContent",
                schema: "careerforge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StableId = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    TechnologyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ObjectivesJson = table.Column<string>(type: "jsonb", nullable: false),
                    PrerequisitesJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ContentType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionedContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VersionedContent_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalSchema: "careerforge",
                        principalTable: "Technologies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RubricDimensions",
                schema: "careerforge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RubricId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubricDimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RubricDimensions_Rubrics_RubricId",
                        column: x => x.RubricId,
                        principalSchema: "careerforge",
                        principalTable: "Rubrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentSections",
                schema: "careerforge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    BodyMarkdown = table.Column<string>(type: "text", nullable: false),
                    CodeLanguage = table.Column<string>(type: "text", nullable: true),
                    CodeSample = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentSections_VersionedContent_ContentId",
                        column: x => x.ContentId,
                        principalSchema: "careerforge",
                        principalTable: "VersionedContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_RubricId",
                schema: "careerforge",
                table: "Questions",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentSections_ContentId_Key",
                schema: "careerforge",
                table: "ContentSections",
                columns: new[] { "ContentId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentSections_ContentId_Order",
                schema: "careerforge",
                table: "ContentSections",
                columns: new[] { "ContentId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RubricDimensions_RubricId_Key",
                schema: "careerforge",
                table: "RubricDimensions",
                columns: new[] { "RubricId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RubricDimensions_RubricId_Order",
                schema: "careerforge",
                table: "RubricDimensions",
                columns: new[] { "RubricId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rubrics_StableId_Version",
                schema: "careerforge",
                table: "Rubrics",
                columns: new[] { "StableId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VersionedContent_Slug_Version",
                schema: "careerforge",
                table: "VersionedContent",
                columns: new[] { "Slug", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VersionedContent_StableId_Version",
                schema: "careerforge",
                table: "VersionedContent",
                columns: new[] { "StableId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VersionedContent_TechnologyId",
                schema: "careerforge",
                table: "VersionedContent",
                column: "TechnologyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Rubrics_RubricId",
                schema: "careerforge",
                table: "Questions",
                column: "RubricId",
                principalSchema: "careerforge",
                principalTable: "Rubrics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    rubric_id uuid := 'a57a68a0-b7aa-4d68-9f96-56be15ca0be1';
                BEGIN
                    IF EXISTS (SELECT 1 FROM careerforge."Questions") THEN
                        INSERT INTO careerforge."Rubrics"
                            ("Id", "StableId", "Version", "Title", "Description", "Status", "CreatedAt", "PublishedAt")
                        VALUES
                            (rubric_id, 'default-technical-answer', 1, 'Teknik cevap değerlendirmesi',
                             'Teknik doğruluk, analiz, trade-off ve iletişim boyutları', 2,
                             CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

                        INSERT INTO careerforge."RubricDimensions"
                            ("Id", "RubricId", "Key", "Label", "Description", "Weight", "Order")
                        VALUES
                            ('528c19ad-666e-4379-8f40-649bc5884580', rubric_id, 'technicalAccuracy',
                             'Teknik doğruluk', 'Teknik doğruluk değerlendirme boyutu', 40, 1),
                            ('5b940604-edee-4274-af7f-f92561d64969', rubric_id, 'analysis',
                             'Analiz', 'Analiz değerlendirme boyutu', 25, 2),
                            ('0258a963-24b7-4831-ad33-d245082b67e2', rubric_id, 'tradeOff',
                             'Trade-off', 'Trade-off değerlendirme boyutu', 20, 3),
                            ('7e83bac1-7528-47dd-9b4c-646183324cdb', rubric_id, 'communication',
                             'İletişim', 'İletişim değerlendirme boyutu', 15, 4);

                        UPDATE careerforge."Questions"
                        SET "RubricId" = rubric_id,
                            "Status" = 2,
                            "PublishedAt" = COALESCE("PublishedAt", CURRENT_TIMESTAMP);
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Rubrics_RubricId",
                schema: "careerforge",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "ContentSections",
                schema: "careerforge");

            migrationBuilder.DropTable(
                name: "RubricDimensions",
                schema: "careerforge");

            migrationBuilder.DropTable(
                name: "VersionedContent",
                schema: "careerforge");

            migrationBuilder.DropTable(
                name: "Rubrics",
                schema: "careerforge");

            migrationBuilder.DropIndex(
                name: "IX_Questions_RubricId",
                schema: "careerforge",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                schema: "careerforge",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "RubricId",
                schema: "careerforge",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "careerforge",
                table: "Questions");
        }
    }
}
