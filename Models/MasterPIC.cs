using System.ComponentModel.DataAnnotations;

namespace BarangKeluarDinas.Models;

public class MasterPIC
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string NamaLengkap { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Departemen { get; set; } = "IT"; 
    
    public bool IsActive { get; set; } = true; 
}