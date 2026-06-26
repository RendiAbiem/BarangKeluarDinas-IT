using System.ComponentModel.DataAnnotations;

namespace BarangKeluarDinas.Models;

public class MasterLokasi
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string NamaLokasi { get; set; } = string.Empty; 
}