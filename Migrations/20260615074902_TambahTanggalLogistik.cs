using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangKeluarDinas.Migrations
{
    /// <inheritdoc />
    public partial class TambahTanggalLogistik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalDikembalikan",
                table: "ItemBarangKeluar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalDipakai",
                table: "ItemBarangKeluar",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TanggalDikembalikan",
                table: "ItemBarangKeluar");

            migrationBuilder.DropColumn(
                name: "TanggalDipakai",
                table: "ItemBarangKeluar");
        }
    }
}
