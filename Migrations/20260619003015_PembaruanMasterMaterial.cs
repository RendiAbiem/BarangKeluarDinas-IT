using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangKeluarDinas.Migrations
{
    /// <inheritdoc />
    public partial class PembaruanMasterMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KategoriAsset",
                table: "MasterMaterial");

            migrationBuilder.DropColumn(
                name: "SatuanDasar",
                table: "MasterMaterial");

            migrationBuilder.RenameColumn(
                name: "Stok",
                table: "MasterMaterial",
                newName: "Jumlah");

            migrationBuilder.AddColumn<string>(
                name: "Satuan",
                table: "MasterMaterial",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MasterMaterial",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Satuan",
                table: "MasterMaterial");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MasterMaterial");

            migrationBuilder.RenameColumn(
                name: "Jumlah",
                table: "MasterMaterial",
                newName: "Stok");

            migrationBuilder.AddColumn<string>(
                name: "KategoriAsset",
                table: "MasterMaterial",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SatuanDasar",
                table: "MasterMaterial",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
