using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarangKeluarDinas.Data;
using BarangKeluarDinas.Models;
using Microsoft.AspNetCore.Http; 
using System.IO; 
using System.Text.Json;

namespace BarangKeluarDinas.Controllers;

public class TugasanDinasController : Controller
{
    private readonly AppDbContext _context;

    public TugasanDinasController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchString, string statusFilter)
    {
        var query = _context.TugasanDinas.Include(t => t.SenaraiBarang).AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(t => t.PIC.Contains(searchString) 
                                  || t.Jobdesk.Contains(searchString) 
                                  || t.LokasiDinas.Contains(searchString)
                                  || (t.SRM != null && t.SRM.Contains(searchString))
                                  || (t.PK_GI != null && t.PK_GI.Contains(searchString)));
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "BelumUpdate")
                query = query.Where(t => t.SenaraiBarang.Any(i => i.Status == StatusAliranBarang.DibawaKeluar));
            else if (statusFilter == "Selesai")
                query = query.Where(t => !t.SenaraiBarang.Any(i => i.Status == StatusAliranBarang.DibawaKeluar));
        }

        ViewData["CurrentFilter"] = searchString;
        ViewData["StatusFilter"] = statusFilter;
        
        var data = await query.OrderByDescending(t => t.TanggalDari).ToListAsync();
        return View(data);
    }

    // Model pembantu untuk membaca struktur data JSON (KategoriAsset dihapus)
    public class ItemBawaSubmitModel
    {
        public string NoMaterial { get; set; } = string.Empty;
        public string NamaUnit { get; set; } = string.Empty;
        public int JumlahBawa { get; set; }
        public string STN { get; set; } = string.Empty;
        public string? Barcode { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.PICList = new SelectList(await _context.MasterPIC.Where(p => p.IsActive).ToListAsync(), "NamaLengkap", "NamaLengkap");
        ViewBag.LokasiList = new SelectList(await _context.MasterLokasi.ToListAsync(), "NamaLokasi", "NamaLokasi");
        
        ViewBag.MaterialList = await _context.MasterMaterial.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TugasanDinas tugasan, string itemsJson)
    {
        List<ItemBawaSubmitModel> rows = new();
        
        if (tugasan.BawaBarang && !string.IsNullOrEmpty(itemsJson))
        {
            try
            {
                rows = JsonSerializer.Deserialize<List<ItemBawaSubmitModel>>(itemsJson) ?? new();
                
                var simulasiStokGudang = new Dictionary<string, int>();
                foreach (var row in rows)
                {
                    if (!simulasiStokGudang.ContainsKey(row.NoMaterial))
                    {
                        var materialCheck = await _context.MasterMaterial.AsNoTracking().FirstOrDefaultAsync(m => m.NoMaterial == row.NoMaterial);
                        simulasiStokGudang[row.NoMaterial] = materialCheck?.Jumlah ?? 0; // Menggunakan .Jumlah
                    }

                    simulasiStokGudang[row.NoMaterial] -= row.JumlahBawa;

                    if (simulasiStokGudang[row.NoMaterial] < 0)
                    {
                        var materialAsli = await _context.MasterMaterial.AsNoTracking().FirstOrDefaultAsync(m => m.NoMaterial == row.NoMaterial);
                        ModelState.AddModelError("", $"Stok material '{row.NamaUnit}' tidak mencukupi untuk dikeluarkan. Sisa stok aktual di gudang saat ini: {materialAsli?.Jumlah ?? 0} {row.STN}.");
                    }
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Format data keranjang barang tidak valid.");
            }
        }

        if (ModelState.IsValid)
        {
            tugasan.Id = Guid.NewGuid(); 
            _context.Add(tugasan);

            if (tugasan.BawaBarang && rows.Any())
            {
                foreach (var row in rows)
                {
                    var newItem = new ItemBarangKeluar
                    {
                        Id = Guid.NewGuid(),
                        TugasanDinasId = tugasan.Id,
                        NoMaterial = row.NoMaterial,
                        NamaUnit = row.NamaUnit,
                        JumlahBawa = row.JumlahBawa,
                        STN = row.STN,
                        Barcode = string.IsNullOrWhiteSpace(row.Barcode) ? null : row.Barcode,
                        Status = StatusAliranBarang.DibawaKeluar
                    };
                    _context.Add(newItem);

                    var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == row.NoMaterial);
                    if (material != null)
                    {
                        material.Jumlah -= row.JumlahBawa; // Menggunakan .Jumlah
                        _context.Update(material);
                    }
                }
            }

            await _context.SaveChangesAsync(); 
            TempData["Success"] = "Data dinas baru beserta daftar barang bawaan berhasil diproses.";
            return RedirectToAction(nameof(Index)); 
        }
        
        ViewBag.PICList = new SelectList(await _context.MasterPIC.Where(p => p.IsActive).ToListAsync(), "NamaLengkap", "NamaLengkap", tugasan.PIC);
        ViewBag.LokasiList = new SelectList(await _context.MasterLokasi.ToListAsync(), "NamaLokasi", "NamaLokasi", tugasan.LokasiDinas);
        ViewBag.MaterialList = await _context.MasterMaterial.ToListAsync();
        return View(tugasan);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var tugasan = await _context.TugasanDinas
            .Include(t => t.SenaraiBarang) 
            .FirstOrDefaultAsync(m => m.Id == id);

        if (tugasan == null) return NotFound();

        ViewBag.MaterialList = await _context.MasterMaterial.ToListAsync();

        return View(tugasan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TambahMaterialDetail(Guid tugasanId, string itemsJson)
    {
        if (string.IsNullOrEmpty(itemsJson))
        {
            TempData["Error"] = "Tidak ada material tambahan yang dipilih.";
            return RedirectToAction(nameof(Details), new { id = tugasanId });
        }

        try
        {
            var rows = JsonSerializer.Deserialize<List<ItemBawaSubmitModel>>(itemsJson) ?? new();
            
            foreach (var row in rows)
            {
                var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == row.NoMaterial);
                if (material == null || material.Jumlah < row.JumlahBawa) // Menggunakan .Jumlah
                {
                    TempData["Error"] = $"Gagal menambah barang. Stok '{row.NamaUnit}' tidak mencukupi atau barang tidak ditemukan.";
                    return RedirectToAction(nameof(Details), new { id = tugasanId });
                }

                var newItem = new ItemBarangKeluar
                {
                    Id = Guid.NewGuid(),
                    TugasanDinasId = tugasanId,
                    NoMaterial = row.NoMaterial,
                    NamaUnit = row.NamaUnit,
                    JumlahBawa = row.JumlahBawa,
                    STN = row.STN,
                    Barcode = string.IsNullOrWhiteSpace(row.Barcode) ? null : row.Barcode,
                    Status = StatusAliranBarang.DibawaKeluar 
                };
                _context.Add(newItem);

                material.Jumlah -= row.JumlahBawa; // Menggunakan .Jumlah
                _context.Update(material);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Material susulan berhasil ditambahkan ke dalam daftar bawaan dinas.";
        }
        catch (Exception)
        {
            TempData["Error"] = "Terjadi kesalahan sistem saat memproses kalkulasi data barang tambahan.";
        }

        return RedirectToAction(nameof(Details), new { id = tugasanId });
    }

    [HttpGet]
    public async Task<IActionResult> ExportWord(Guid id)
    {
        var tugasan = await _context.TugasanDinas
            .Include(t => t.SenaraiBarang) 
            .FirstOrDefaultAsync(m => m.Id == id);

        if (tugasan == null) return NotFound();

        string namaFile = $"Laporan_Tugas_{tugasan.PIC.Replace(" ", "_")}_{tugasan.TanggalDari:ddMMMyyyy}.doc";
        
        Response.Headers.Append("Content-Disposition", $"attachment; filename={namaFile}");
        Response.ContentType = "application/vnd.ms-word";

        return View(tugasan);
    }

    [HttpGet]
    public async Task<IActionResult> Preview(Guid id)
    {
        var tugasan = await _context.TugasanDinas
            .Include(t => t.SenaraiBarang) 
            .FirstOrDefaultAsync(m => m.Id == id);

        if (tugasan == null) return NotFound();

        return View(tugasan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelesaikanTugas(Guid id)
    {
        var tugasan = await _context.TugasanDinas.Include(t => t.SenaraiBarang).FirstOrDefaultAsync(t => t.Id == id);
        if (tugasan == null) return NotFound();

        if (tugasan.SenaraiBarang != null && tugasan.SenaraiBarang.Any(b => b.Status == StatusAliranBarang.DibawaKeluar))
        {
            TempData["Error"] = "Gagal menyelesaikan penugasan. Masih ada barang bawaan dengan status pending yang belum dikelola laporannya.";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        TempData["Success"] = "Penugasan dinas dinyatakan telah selesai sepenuhnya.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var tugasan = await _context.TugasanDinas.FindAsync(id);
        if (tugasan == null) return NotFound();

        ViewBag.PICList = new SelectList(await _context.MasterPIC.Where(p => p.IsActive).ToListAsync(), "NamaLengkap", "NamaLengkap", tugasan.PIC);
        ViewBag.LokasiList = new SelectList(await _context.MasterLokasi.ToListAsync(), "NamaLokasi", "NamaLokasi", tugasan.LokasiDinas);

        return View(tugasan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TugasanDinas tugasan)
    {
        if (id != tugasan.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var originalData = await _context.TugasanDinas.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if(originalData != null)
            {
                tugasan.PathDokumenBukti = originalData.PathDokumenBukti;
                tugasan.SRM = originalData.SRM;
                tugasan.PK_GI = originalData.PK_GI;
            }

            _context.Update(tugasan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.PICList = new SelectList(await _context.MasterPIC.Where(p => p.IsActive).ToListAsync(), "NamaLengkap", "NamaLengkap", tugasan.PIC);
        ViewBag.LokasiList = new SelectList(await _context.MasterLokasi.ToListAsync(), "NamaLokasi", "NamaLokasi", tugasan.LokasiDinas);

        return View(tugasan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tugasan = await _context.TugasanDinas.FindAsync(id);
        if (tugasan != null)
        {
            if (!string.IsNullOrEmpty(tugasan.PathDokumenBukti))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bukti", tugasan.PathDokumenBukti);
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (IOException) { }
                }
            }

            _context.TugasanDinas.Remove(tugasan);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadBukti(Guid id, IFormFile file)
    {
        var tugasan = await _context.TugasanDinas.FindAsync(id);
        if (tugasan == null) return NotFound();

        if (file != null && file.Length > 0)
        {
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Format file tidak didukung. Sistem hanya menerima file PDF, JPG, atau PNG.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bukti");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            if (!string.IsNullOrEmpty(tugasan.PathDokumenBukti))
            {
                var oldFilePath = Path.Combine(uploadsFolder, tugasan.PathDokumenBukti);
                if (System.IO.File.Exists(oldFilePath))
                {
                    try { System.IO.File.Delete(oldFilePath); } catch (IOException) { }
                }
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            tugasan.PathDokumenBukti = uniqueFileName;
            _context.Update(tugasan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Dokumen bukti berhasil diperbarui dan disimpan dalam sistem.";
        }
        else
        {
            TempData["Error"] = "Gagal memproses file. Pastikan Anda telah memilih dokumen sebelum mengunggah.";
        }

        return RedirectToAction(nameof(Details), new { id = id });
    }
}