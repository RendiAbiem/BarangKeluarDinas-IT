using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangKeluarDinas.Models;

public class ItemBarangKeluar
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Foreign Key TugasanDinas
    public Guid TugasanDinasId { get; set; }
    [ForeignKey("TugasanDinasId")]
    public virtual TugasanDinas? TugasanDinas { get; set; }

    // -- Barang Keluar --
    [StringLength(150)]
    public string NamaUnit { get; set; } = string.Empty;
    
    // [UPDATE] Membuat No Material wajib diisi
    [Required(ErrorMessage = "Nomor Material wajib diisi!")]
    [StringLength(50)]
    public string NoMaterial { get; set; } = string.Empty;
    
    // [UPDATE] Menambahkan Kategori Asset (Default: Non Asset)
    [StringLength(50)]
    public string KategoriAsset { get; set; } = "Non Asset";

    public int JumlahBawa { get; set; }
    
    [StringLength(20)]
    public string STN { get; set; } = string.Empty;
    
    // Barcode tetap nullable (?) karena ada barang Non Asset yang tidak punya serial number
    [StringLength(100)]
    public string? Barcode { get; set; } 

    // -- Catatan Pengembalian --
    [StringLength(150)]
    public string? UnitDipakai { get; set; }
    
    public int? JumlahDipakai { get; set; }
    
    [StringLength(50)]
    public string? NoMaterialPengembalian { get; set; }
    
    [StringLength(50)]
    public string? PK_GI { get; set; }
    
    [StringLength(50)]
    public string? SRM { get; set; }

    // -- Tambahan --
    [StringLength(150)]
    public string? UnitLama { get; set; }
    
    public string? Keterangan { get; set; }

    public StatusAliranBarang Status { get; set; } = StatusAliranBarang.DibawaKeluar;

    public DateTime? TanggalDipakai { get; set; }
    public DateTime? TanggalDikembalikan { get; set; }
}

public enum StatusAliranBarang
{
    DibawaKeluar,           
    TerpakaiKategoriSRM,    
    KembaliGudangIT         
}