using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CancellationDate",
                table: "Policies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Policies",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    PolicyReference = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.PolicyReference);
                    table.ForeignKey(
                        name: "FK_Refunds_Policies_PolicyReference",
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
                name: "Refunds");

            migrationBuilder.DropColumn(
                name: "CancellationDate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Policies");
        }
    }
}
