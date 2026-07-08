using System.ComponentModel.DataAnnotations;

namespace BarangKeluarDinas.Models;

public class DataBarcode
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string NoMaterial { get; set; } = string.Empty;

    [Required]
    public string Barcode { get; set; } = string.Empty;

    public DateTime TanggalDidaftarkan { get; set; } = DateTime.Now;
}