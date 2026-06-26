using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangKeluarDinas.Data;
using BarangKeluarDinas.Models;

namespace BarangKeluarDinas.Controllers;

public class MasterDataController : Controller
{
    private readonly AppDbContext _context;

    public MasterDataController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchMaterial, string searchPIC, string searchLokasi)
    {
        var materialQuery = _context.MasterMaterial.AsQueryable();
        if (!string.IsNullOrEmpty(searchMaterial))
        {
            materialQuery = materialQuery.Where(m => m.NoMaterial.Contains(searchMaterial) || m.NamaUnit.Contains(searchMaterial));
        }

        var picQuery = _context.MasterPIC.AsQueryable();
        if (!string.IsNullOrEmpty(searchPIC))
        {
            picQuery = picQuery.Where(p => p.NamaLengkap.Contains(searchPIC) || p.Departemen.Contains(searchPIC));
        }

        var lokasiQuery = _context.MasterLokasi.AsQueryable();
        if (!string.IsNullOrEmpty(searchLokasi))
        {
            lokasiQuery = lokasiQuery.Where(l => l.NamaLokasi.Contains(searchLokasi));
        }

        ViewBag.ListMaterial = await materialQuery.ToListAsync();
        ViewBag.ListPIC = await picQuery.ToListAsync();
        ViewBag.ListLokasi = await lokasiQuery.ToListAsync();

        ViewData["CurrentMaterialFilter"] = searchMaterial;
        ViewData["CurrentPICFilter"] = searchPIC;
        ViewData["CurrentLokasiFilter"] = searchLokasi;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> TambahMaterial(MasterMaterial material)
    {
        if (ModelState.IsValid)
        {
            var exists = await _context.MasterMaterial.AnyAsync(m => m.NoMaterial == material.NoMaterial);
            if (!exists)
            {
                try
                {
                    _context.MasterMaterial.Add(material);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Material {material.NamaUnit} berhasil disimpan!";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Gagal menyimpan ke database. Pastikan Anda sudah menjalankan Update Database Migration untuk kolom Jumlah/Status.";
                }
            }
            else
            {
                TempData["Error"] = "Gagal: Nomor Material tersebut sudah terdaftar di sistem!";
            }
        }
        else
        {
            TempData["Error"] = "Gagal: Format isian tidak valid. Cek kembali form Anda.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStokMaterial(string noMaterial, int tambahanStok)
    {
        var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == noMaterial);
        
        if (material != null)
        {
            try
            {
                // PERBAIKAN: Menggunakan properti .Jumlah menggantikan .Stok
                material.Jumlah += tambahanStok; 
                _context.Update(material);
                await _context.SaveChangesAsync();
                
                // PERBAIKAN: Menggunakan properti .Jumlah untuk pesan notifikasi
                TempData["Success"] = $"Berhasil menambah kuantitas barang ke dalam data {material.NamaUnit}. Total terbaru: {material.Jumlah}.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Terjadi kesalahan sistem saat mencoba menambah kuantitas barang.";
            }
        }
        else
        {
            TempData["Error"] = "Data material tidak ditemukan!";
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> TambahPIC(MasterPIC pic)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.MasterPIC.Add(pic);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"PIC {pic.NamaLengkap} berhasil disimpan!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Gagal menyimpan data PIC ke database.";
            }
        }
        else
        {
            TempData["Error"] = "Gagal: Form PIC tidak valid.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> TambahLokasi(MasterLokasi lokasi)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.MasterLokasi.Add(lokasi);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Lokasi {lokasi.NamaLokasi} berhasil disimpan!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Gagal menyimpan data Lokasi ke database.";
            }
        }
        else
        {
            TempData["Error"] = "Gagal: Form Lokasi tidak valid.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MasterMaterial masterMaterial)
    {
        // Cek apakah NoMaterial sudah terdaftar
        bool isExist = await _context.MasterMaterial
                            .AnyAsync(m => m.NoMaterial == masterMaterial.NoMaterial);

        if (isExist)
        {
            ModelState.AddModelError("NoMaterial", "Nomor Material ini sudah terdaftar di sistem.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(masterMaterial);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Data material baru berhasil disimpan.";
            return RedirectToAction(nameof(Index));
        }
        return View(masterMaterial);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatusMaterial(string noMaterial)
    {
        var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == noMaterial);
        
        if (material != null)
        {
            try
            {
                // Jika Y ubah ke N, jika selain Y ubah ke Y
                material.Status = (material.Status == "Y") ? "N" : "Y"; 
                
                _context.Update(material);
                await _context.SaveChangesAsync();
                
                string statusBaru = material.Status == "Y" ? "Aktif" : "Non-Aktif";
                TempData["Success"] = $"Status {material.NamaUnit} berhasil diubah menjadi {statusBaru}.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Terjadi kesalahan saat mengubah status material.";
            }
        }
        else
        {
            TempData["Error"] = "Data material tidak ditemukan!";
        }
        
        return RedirectToAction(nameof(Index));
    }
}