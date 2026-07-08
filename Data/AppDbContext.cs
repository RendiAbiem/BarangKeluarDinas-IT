using Microsoft.EntityFrameworkCore;
using BarangKeluarDinas.Models; // Sesuaikan dengan nama project Anda

namespace BarangKeluarDinas.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TugasanDinas> TugasanDinas { get; set; }
    public DbSet<ItemBarangKeluar> ItemBarangKeluar { get; set; }
    public DbSet<MasterLokasi> MasterLokasi { get; set; }
    public DbSet<MasterPIC> MasterPIC { get; set; }
    public DbSet<MasterMaterial> MasterMaterial { get; set; }
    public DbSet<HistoryMaterial> HistoryMaterial { get; set; }
    public DbSet<DataBarcode> DataBarcode { get; set; }
}