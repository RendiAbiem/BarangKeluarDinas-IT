using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangKeluarDinas.Migrations
{
    /// <inheritdoc />
    public partial class InitialSQLServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataBarcode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoMaterial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalDidaftarkan = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBarcode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoryMaterial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoMaterial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tanggal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JenisAktivitas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerubahanJumlah = table.Column<int>(type: "int", nullable: false),
                    SaldoAkhir = table.Column<int>(type: "int", nullable: false),
                    Plant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StorageLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Keterangan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryMaterial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterLokasi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamaLokasi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterLokasi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterMaterial",
                columns: table => new
                {
                    NoMaterial = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NamaUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Satuan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Jumlah = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterMaterial", x => x.NoMaterial);
                });

            migrationBuilder.CreateTable(
                name: "MasterPIC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamaLengkap = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Departemen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterPIC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TugasanDinas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TanggalDari = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TanggalKe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PIC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Jobdesk = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LokasiDinas = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BawaBarang = table.Column<bool>(type: "bit", nullable: false),
                    PathDokumenBukti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SRM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PK_GI = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TugasanDinas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemBarangKeluar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TugasanDinasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NamaUnit = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NoMaterial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KategoriAsset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JumlahBawa = table.Column<int>(type: "int", nullable: false),
                    STN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitDipakai = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    JumlahDipakai = table.Column<int>(type: "int", nullable: true),
                    NoMaterialPengembalian = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PK_GI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SRM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UnitLama = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Keterangan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TanggalDipakai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TanggalDikembalikan = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "DataBarcode");

            migrationBuilder.DropTable(
                name: "HistoryMaterial");

            migrationBuilder.DropTable(
                name: "ItemBarangKeluar");

            migrationBuilder.DropTable(
                name: "MasterLokasi");

            migrationBuilder.DropTable(
                name: "MasterMaterial");

            migrationBuilder.DropTable(
                name: "MasterPIC");

            migrationBuilder.DropTable(
                name: "TugasanDinas");
        }
    }
}
