using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerApp.Migrations
{
    public partial class paycheck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Persons",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Persons",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualGrossPay",
                table: "Persons",
                type: "decimal(32, 2)",
                nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Persons_EmployeeId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "AnnualGrossPay",
                table: "Persons");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Persons_EmployeeId",
                table: "Persons",
                column: "EmployeeId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
