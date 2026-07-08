using System.ComponentModel.DataAnnotations;

namespace BarangKeluarDinas.Models;

public class MasterMaterial
{
    [Key]
    public string NoMaterial { get; set; } = string.Empty;
    public string NamaUnit { get; set; } = string.Empty;
    public string Satuan { get; set; } = string.Empty; 
    public int Jumlah { get; set; } 
    public string Status { get; set; } = "Y"; 
}