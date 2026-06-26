using System.ComponentModel.DataAnnotations;

namespace BarangKeluarDinas.Models;

public class TugasanDinas
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime TanggalDari { get; set; }

    [Required]
    public DateTime TanggalKe { get; set; }

    [Required]
    [StringLength(100)]
    public string PIC { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Jobdesk { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string LokasiDinas { get; set; } = string.Empty;

    public bool BawaBarang { get; set; }

    public virtual ICollection<ItemBarangKeluar> SenaraiBarang { get; set; } = new List<ItemBarangKeluar>();

    public string? PathDokumenBukti { get; set; }

    [StringLength(100)]
    public string? SRM { get; set; }

    [StringLength(100)]
    public string? PK_GI { get; set; }
}