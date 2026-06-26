using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangKeluarDinas.Migrations
{
    /// <inheritdoc />
    public partial class TambahStokMasterMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stok",
                table: "MasterMaterial",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stok",
                table: "MasterMaterial");
        }
    }
}
