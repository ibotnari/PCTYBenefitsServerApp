using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerApp.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Benefits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsEnabled = table.Column<bool>(nullable: false),
                    AnnualCost = table.Column<decimal>(type: "decimal(8, 2)", nullable: false),
                    Description = table.Column<string>(maxLength: 100, nullable: false),
                    Discriminator = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benefits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    DependentRelationshipToEmployee = table.Column<int>(nullable: true),
                    EmployeeId = table.Column<int>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Persons_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BenefitDiscount",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Percent = table.Column<decimal>(type: "decimal(5, 4)", nullable: false),
                    BenefitId = table.Column<int>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    NameStartsWith = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenefitDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BenefitDiscount_Benefits_BenefitId",
                        column: x => x.BenefitId,
                        principalTable: "Benefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Paychecks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(8, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(8, 2)", nullable: false),
                    BenefitsCost = table.Column<decimal>(type: "decimal(8, 2)", nullable: true),
                    BenefitsCostCalculationDate = table.Column<DateTime>(nullable: true),
                    SentDate = table.Column<DateTime>(nullable: true),
                    EmployeeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paychecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paychecks_Persons_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaycheckBenefitCost",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    BenefitReceiverId = table.Column<int>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(8, 2)", nullable: false),
                    AmountBeforeDiscounts = table.Column<decimal>(type: "decimal(8, 2)", nullable: false),
                    PaycheckId = table.Column<int>(nullable: false),
                    BenefitId = table.Column<int>(nullable: true),
                    ResidualAmount = table.Column<decimal>(type: "decimal(32,24)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaycheckBenefitCost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaycheckBenefitCost_Benefits_BenefitId",
                        column: x => x.BenefitId,
                        principalTable: "Benefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaycheckBenefitCost_Persons_BenefitReceiverId",
                        column: x => x.BenefitReceiverId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaycheckBenefitCost_Paychecks_PaycheckId",
                        column: x => x.PaycheckId,
                        principalTable: "Paychecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenefitDiscount_BenefitId",
                table: "BenefitDiscount",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckBenefitCost_BenefitId",
                table: "PaycheckBenefitCost",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckBenefitCost_BenefitReceiverId",
                table: "PaycheckBenefitCost",
                column: "BenefitReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckBenefitCost_PaycheckId",
                table: "PaycheckBenefitCost",
                column: "PaycheckId");

            migrationBuilder.CreateIndex(
                name: "IX_Paychecks_EmployeeId",
                table: "Paychecks",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_EmployeeId",
                table: "Persons",
                column: "EmployeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenefitDiscount");

            migrationBuilder.DropTable(
                name: "PaycheckBenefitCost");

            migrationBuilder.DropTable(
                name: "Benefits");

            migrationBuilder.DropTable(
                name: "Paychecks");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
