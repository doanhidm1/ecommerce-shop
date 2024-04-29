using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class Update_Order_Address : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Bills");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Bills",
                newName: "ExactAddress");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Bills",
                newName: "WardCommune");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Bills",
                newName: "DistrictTown");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Bills",
                newName: "CustomerName");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Bills",
                newName: "CityProvince");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Bills",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WardCommune",
                table: "Bills",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "ExactAddress",
                table: "Bills",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "DistrictTown",
                table: "Bills",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "Bills",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "CityProvince",
                table: "Bills",
                newName: "City");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Bills",
                type: "nvarchar(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");

            migrationBuilder.AddColumn<int>(
                name: "ZipCode",
                table: "Bills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
