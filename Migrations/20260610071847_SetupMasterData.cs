using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangKeluarDinas.Migrations
{
    /// <inheritdoc />
    public partial class SetupMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MasterLokasi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NamaLokasi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterLokasi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterMaterial",
                columns: table => new
                {
                    NoMaterial = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NamaUnit = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    KategoriAsset = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SatuanDasar = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterMaterial", x => x.NoMaterial);
                });

            migrationBuilder.CreateTable(
                name: "MasterPIC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NamaLengkap = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Departemen = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterPIC", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterLokasi");

            migrationBuilder.DropTable(
                name: "MasterMaterial");

            migrationBuilder.DropTable(
                name: "MasterPIC");
        }
    }
}
