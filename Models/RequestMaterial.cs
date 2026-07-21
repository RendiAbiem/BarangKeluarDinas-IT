using System.ComponentModel.DataAnnotations;

namespace BarangKeluarDinas.Models
{
    public class RequestMaterial
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string NamaPemohon { get; set; } = string.Empty;
        
        public string Departemen { get; set; } = "SPOKE / IT";

        [Required]
        public string JenisRequest { get; set; } = string.Empty; // Isi: "Barang Lama" atau "Barang Baru"

        // Nullable karena jika "Barang Baru", belum ada nomor materialnya
        public string? NoMaterial { get; set; }

        [Required]
        public string NamaBarang { get; set; } = string.Empty;

        [Required]
        public int Jumlah { get; set; }

        [Required]
        public string Satuan { get; set; } = string.Empty;

        [Required]
        public string AlasanKebutuhan { get; set; } = string.Empty;

        public DateTime TanggalRequest { get; set; } = DateTime.Now;

        // Status default saat pertama kali dibuat
        public string Status { get; set; } = "Pending"; 
        
        public string? CatatanAdmin { get; set; }
    }
}