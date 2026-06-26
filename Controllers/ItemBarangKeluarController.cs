using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangKeluarDinas.Data;
using BarangKeluarDinas.Models;
using System.Text.Json;

namespace BarangKeluarDinas.Controllers;

public class ItemBarangKeluarController : Controller
{
    private readonly AppDbContext _context;

    public ItemBarangKeluarController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid tugasanId)
    {
        ViewBag.TugasanDinasId = tugasanId;
        ViewBag.MaterialList = await _context.MasterMaterial.ToListAsync();
        return View();
    }

    public class ItemSubmitModel
    {
        public string NoMaterial { get; set; } = string.Empty;
        public string NamaUnit { get; set; } = string.Empty;
        public int JumlahBawa { get; set; }
        public string STN { get; set; } = string.Empty;
        public string? Barcode { get; set; }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid TugasanDinasId, string itemsJson)
    {
        if (!string.IsNullOrEmpty(itemsJson))
        {
            try
            {
                var rows = JsonSerializer.Deserialize<List<ItemSubmitModel>>(itemsJson);
                
                if (rows != null && rows.Any())
                {
                    // TAHAP 1: SIMULASI & VALIDASI STOK DATABASE
                    var simulasiStokGudang = new Dictionary<string, int>();

                    foreach (var row in rows)
                    {
                        if (!simulasiStokGudang.ContainsKey(row.NoMaterial))
                        {
                            var materialCheck = await _context.MasterMaterial.AsNoTracking().FirstOrDefaultAsync(m => m.NoMaterial == row.NoMaterial);
                            simulasiStokGudang[row.NoMaterial] = materialCheck?.Jumlah ?? 0;
                        }

                        simulasiStokGudang[row.NoMaterial] -= row.JumlahBawa;

                        if (simulasiStokGudang[row.NoMaterial] < 0)
                        {
                            var materialAsli = await _context.MasterMaterial.AsNoTracking().FirstOrDefaultAsync(m => m.NoMaterial == row.NoMaterial);
                            int totalDiminta = rows.Where(r => r.NoMaterial == row.NoMaterial).Sum(r => r.JumlahBawa);

                            TempData["Error"] = $"TRANSAKSI DITOLAK: Stok material '{row.NamaUnit}' di Gudang IT tidak mencukupi! Sisa Stok Aktual: {materialAsli?.Jumlah ?? 0} {row.STN}, sedangkan Anda mencoba mengeluarkan total: {totalDiminta} {row.STN}.";
                            
                            ViewBag.TugasanDinasId = TugasanDinasId;
                            ViewBag.MaterialList = await _context.MasterMaterial.ToListAsync();
                            return View();
                        }
                    }

                    // TAHAP 2: EKSEKUSI DATA
                    foreach (var row in rows)
                    {
                        var newItem = new ItemBarangKeluar
                        {
                            Id = Guid.NewGuid(),
                            TugasanDinasId = TugasanDinasId,
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
                            material.Jumlah -= row.JumlahBawa;
                            _context.Update(material);
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Berhasil! {rows.Count} item material telah diproses keluar dan stok gudang berhasil diperbarui.";
                    return RedirectToAction("Details", "TugasanDinas", new { id = TugasanDinasId });
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Terjadi kegagalan sistem saat mengurai data daftar keranjang barang.";
            }
        }

        ViewBag.TugasanDinasId = TugasanDinasId;
        ViewBag.MaterialList = await _context.MasterMaterial.ToListAsync();
        TempData["Error"] = "Keranjang barang Anda masih kosong! Gagal memproses penyimpanan.";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> UpdatePengembalian(Guid? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ItemBarangKeluar.FindAsync(id);
        if (item == null) return NotFound();

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePengembalian(Guid id, ItemBarangKeluar model, int sisaKembali = 0)
    {
        if (id != model.Id) return NotFound();

        try
        {
            var itemToUpdate = await _context.ItemBarangKeluar.FindAsync(id);
            if (itemToUpdate == null) return NotFound();

            var statusLama = itemToUpdate.Status;
            int originalJumlahBawa = itemToUpdate.JumlahBawa;
            var statusBaru = model.Status;

            var tugasUtama = await _context.TugasanDinas.AsNoTracking().FirstOrDefaultAsync(t => t.Id == itemToUpdate.TugasanDinasId);

            // LOGIKA PEMECAHAN STOK OTOMATIS (AUTO-SPLIT)
            if (originalJumlahBawa > 1 && statusBaru == StatusAliranBarang.TerpakaiKategoriSRM && sisaKembali > 0 && sisaKembali < originalJumlahBawa)
            {
                int jumlahBenarTerpakai = originalJumlahBawa - sisaKembali;

                itemToUpdate.JumlahBawa = jumlahBenarTerpakai;
                itemToUpdate.Status = model.Status;
                itemToUpdate.SRM = string.IsNullOrWhiteSpace(model.SRM) ? tugasUtama?.SRM : model.SRM;
                itemToUpdate.Keterangan = model.Keterangan; 
                itemToUpdate.TanggalDipakai = model.TanggalDipakai;
                itemToUpdate.TanggalDikembalikan = null; 

                _context.Update(itemToUpdate);

                var itemKembali = new ItemBarangKeluar
                {
                    Id = Guid.NewGuid(),
                    TugasanDinasId = itemToUpdate.TugasanDinasId,
                    NoMaterial = itemToUpdate.NoMaterial,
                    NamaUnit = itemToUpdate.NamaUnit,
                    STN = itemToUpdate.STN,
                    Barcode = itemToUpdate.Barcode,
                    JumlahBawa = sisaKembali,
                    Status = StatusAliranBarang.KembaliGudangIT, 
                    Keterangan = "Auto-Split: Sisa material tidak terpakai, dikembalikan ke gudang.",
                    TanggalDipakai = null,
                    TanggalDikembalikan = model.TanggalDikembalikan
                };
                _context.ItemBarangKeluar.Add(itemKembali);

                var materialCheck = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == itemToUpdate.NoMaterial);
                if (materialCheck != null)
                {
                    materialCheck.Jumlah += sisaKembali;
                    _context.Update(materialCheck);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Status berhasil dipecah: {jumlahBenarTerpakai} Terpakai, {sisaKembali} Kembali ke Gudang.";
                return RedirectToAction("Details", "TugasanDinas", new { id = itemToUpdate.TugasanDinasId });
            }

            // LOGIKA PEMBARUAN NORMAL
            itemToUpdate.Status = model.Status;
            itemToUpdate.JumlahDipakai = model.JumlahDipakai;
            
            itemToUpdate.SRM = string.IsNullOrWhiteSpace(model.SRM) ? tugasUtama?.SRM : model.SRM;
            itemToUpdate.Keterangan = model.Keterangan;

            if (statusBaru == StatusAliranBarang.TerpakaiKategoriSRM)
            {
                itemToUpdate.TanggalDipakai = model.TanggalDipakai;
                itemToUpdate.TanggalDikembalikan = null;
            }
            else if (statusBaru == StatusAliranBarang.KembaliGudangIT)
            {
                itemToUpdate.TanggalDipakai = null; 
                itemToUpdate.TanggalDikembalikan = model.TanggalDikembalikan;
            }
            else
            {
                itemToUpdate.TanggalDipakai = null;
                itemToUpdate.TanggalDikembalikan = null;
            }

            _context.Update(itemToUpdate);

            if (statusBaru != statusLama)
            {
                var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == itemToUpdate.NoMaterial);
                if (material != null)
                {
                    if (statusBaru == StatusAliranBarang.KembaliGudangIT) 
                    {
                        material.Jumlah += itemToUpdate.JumlahBawa;
                    }
                    else if (statusLama == StatusAliranBarang.KembaliGudangIT)
                    {
                        material.Jumlah -= itemToUpdate.JumlahBawa;
                    }
                    _context.Update(material);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Status barang dan pencatatan waktu aktual berhasil diperbarui.";

            return RedirectToAction("Details", "TugasanDinas", new { id = itemToUpdate.TugasanDinasId });
        }
        catch (DbUpdateConcurrencyException)
        {
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickReturn(Guid id)
    {
        var item = await _context.ItemBarangKeluar.FindAsync(id);
        if (item == null) return NotFound();

        if (item.Status == StatusAliranBarang.DibawaKeluar)
        {
            item.Status = StatusAliranBarang.KembaliGudangIT;
            item.Keterangan = "Pengembalian Cepat (Barang utuh tidak terpakai)";
            item.TanggalDikembalikan = DateTime.Now;
            item.TanggalDipakai = null;

            _context.Update(item);

            var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == item.NoMaterial);
            if (material != null)
            {
                material.Jumlah += item.JumlahBawa;
                _context.Update(material);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Barang {item.NamaUnit} berhasil ditarik dan stok gudang telah diperbarui.";
        }
        else
        {
            TempData["Error"] = "Barang ini sudah diproses sebelumnya dan tidak bisa ditarik cepat.";
        }

        return RedirectToAction("Details", "TugasanDinas", new { id = item.TugasanDinasId });
    }
}