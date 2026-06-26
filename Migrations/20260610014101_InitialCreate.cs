using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangKeluarDinas.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TugasanDinas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TanggalDari = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TanggalKe = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PIC = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Jobdesk = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LokasiDinas = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    BawaBarang = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TugasanDinas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemBarangKeluar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TugasanDinasId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NamaUnit = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    NoMaterial = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    JumlahBawa = table.Column<int>(type: "INTEGER", nullable: false),
                    STN = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UnitDipakai = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    JumlahDipakai = table.Column<int>(type: "INTEGER", nullable: true),
                    NoMaterialPengembalian = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PK_GI = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SRM = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UnitLama = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Keterangan = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemBarangKeluar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemBarangKeluar_TugasanDinas_TugasanDinasId",
                        column: x => x.TugasanDinasId,
                        principalTable: "TugasanDinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemBarangKeluar_TugasanDinasId",
                table: "ItemBarangKeluar",
                column: "TugasanDinasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemBarangKeluar");

            migrationBuilder.DropTable(
                name: "TugasanDinas");
        }
    }
}
