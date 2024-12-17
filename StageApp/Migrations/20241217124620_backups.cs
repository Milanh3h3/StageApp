using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StageApp.Migrations
{
    /// <inheritdoc />
    public partial class backups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Organisations");

            migrationBuilder.RenameColumn(
                name: "SerialNumber",
                table: "DeviceBackups",
                newName: "Filepath");

            migrationBuilder.RenameColumn(
                name: "OldName",
                table: "DeviceBackups",
                newName: "BackupName");

            migrationBuilder.AddColumn<int>(
                name: "DevicesAmount",
                table: "DeviceBackups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DevicesAmount",
                table: "DeviceBackups");

            migrationBuilder.RenameColumn(
                name: "Filepath",
                table: "DeviceBackups",
                newName: "SerialNumber");

            migrationBuilder.RenameColumn(
                name: "BackupName",
                table: "DeviceBackups",
                newName: "OldName");

            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Organisations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
