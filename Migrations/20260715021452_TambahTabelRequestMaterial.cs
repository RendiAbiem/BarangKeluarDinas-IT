using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangKeluarDinas.Migrations
{
    /// <inheritdoc />
    public partial class TambahTabelRequestMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestMaterial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NamaPemohon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Departemen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisRequest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoMaterial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamaBarang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Jumlah = table.Column<int>(type: "int", nullable: false),
                    Satuan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlasanKebutuhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalRequest = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CatatanAdmin = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestMaterial", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestMaterial");
        }
    }
}
