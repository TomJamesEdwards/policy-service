using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Reference = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AutoRenew = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasClaims = table.Column<bool>(type: "INTEGER", nullable: false),
                    AddressLine1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AddressLine2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    AddressLine3 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Postcode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Reference);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Reference = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PolicyReference = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => new { x.PolicyReference, x.Reference });
                    table.ForeignKey(
                        name: "FK_Payments_Policies_PolicyReference",
                        column: x => x.PolicyReference,
                        principalTable: "Policies",
                        principalColumn: "Reference",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Policyholders",
                columns: table => new
                {
                    PolicyReference = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policyholders", x => new { x.PolicyReference, x.Id });
                    table.ForeignKey(
                        name: "FK_Policyholders_Policies_PolicyReference",
                        column: x => x.PolicyReference,
                        principalTable: "Policies",
                        principalColumn: "Reference",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Policyholders");

            migrationBuilder.DropTable(
                name: "Policies");
        }
    }
}
