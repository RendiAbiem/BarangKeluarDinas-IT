using System.ComponentModel.DataAnnotations;

namespace BarangKeluarDinas.Models;

public class HistoryMaterial
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string NoMaterial { get; set; } = string.Empty;

    [Required]
    public string NamaUnit { get; set; } = string.Empty;

    [Required]
    public DateTime Tanggal { get; set; } = DateTime.Now;

    // Jenis: "Inbound / Barang Datang", "Outbound Dinas", "Pinjam Luar Dinas", "Koreksi Data", "Unit Rusak"
    [Required]
    public string JenisAktivitas { get; set; } = string.Empty;

    public int PerubahanJumlah { get; set; } // Bisa bernilai positif (stok masuk) atau negatif (stok keluar)

    public int SaldoAkhir { get; set; } // Sisa stok di gudang setelah aktivitas terjadi

    public string? Plant { get; set; } // Contoh: Plant 1, Plant 2, HO

    public string? StorageLocation { get; set; } // Contoh: Sloking / Gudang A, Gudang B

    public string? Keterangan { get; set; } // Detail alasan perubahan data
}