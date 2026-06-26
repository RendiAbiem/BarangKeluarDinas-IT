using System.ComponentModel.DataAnnotations;

namespace BarangKeluarDinas.Models;

public class MasterMaterial
{
    [Key]
    public string NoMaterial { get; set; } = string.Empty;

    public string NamaUnit { get; set; } = string.Empty;

    public string Satuan { get; set; } = string.Empty; // Menggantikan SatuanDasar

    public int Jumlah { get; set; } // Menggantikan Stok

    public string Status { get; set; } = "Y"; // Y: Yes (Aktif), N: No (Non-aktif)
}