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

        // Mengambil 100 log riwayat logistik terakhir untuk ditampilkan di dashboard industri
        ViewBag.ListHistory = await _context.HistoryMaterial.OrderByDescending(h => h.Tanggal).Take(100).ToListAsync();
        ViewBag.ListBarcode = await _context.DataBarcode.ToListAsync();

        ViewData["CurrentMaterialFilter"] = searchMaterial;
        ViewData["CurrentPICFilter"] = searchPIC;
        ViewData["CurrentLokasiFilter"] = searchLokasi;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

                    // LOG HISTORY: Pencatatan registrasi awal material baru
                    var log = new HistoryMaterial
                    {
                        NoMaterial = material.NoMaterial,
                        NamaUnit = material.NamaUnit,
                        JenisAktivitas = "Registrasi Material Baru",
                        PerubahanJumlah = material.Jumlah,
                        SaldoAkhir = material.Jumlah,
                        Keterangan = "Inisialisasi awal material ke dalam master data sistem."
                    };
                    _context.HistoryMaterial.Add(log);

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Material {material.NamaUnit} berhasil disimpan!";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Gagal menyimpan ke database.";
                }
            }
            else
            {
                TempData["Error"] = "Gagal: Nomor Material tersebut sudah terdaftar di sistem!";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStokMaterial(string noMaterial, int tambahanStok)
    {
        var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == noMaterial);
        
        if (material != null)
        {
            try
            {
                material.Jumlah += tambahanStok;
                _context.Update(material);

                // LOG HISTORY: Barang datang (Inbound standar)
                var log = new HistoryMaterial
                {
                    NoMaterial = material.NoMaterial,
                    NamaUnit = material.NamaUnit,
                    JenisAktivitas = "Barang Datang / Inbound",
                    PerubahanJumlah = tambahanStok,
                    SaldoAkhir = material.Jumlah,
                    Keterangan = "Penambahan stok masuk rutin dari pengadaan gudang."
                };
                _context.HistoryMaterial.Add(log);

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Berhasil menambah kuantitas barang ke dalam data {material.NamaUnit}. Total terbaru: {material.Jumlah}.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Terjadi kesalahan sistem saat mencoba menambah kuantitas barang.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    // FUNGSI INDUSTRI BARU: Penyesuaian stok lanjutan (Rusak, Pinjam Luar Dinas, Koreksi Data)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PenyesuaianKoreksiMaterial(string noMaterial, string jenisAktivitas, int kuantitasPerubahan, string plant, string storageLocation, string keterangan)
    {
        var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == noMaterial);
        if (material == null)
        {
            TempData["Error"] = "Material tidak ditemukan!";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Update jumlah stok fisik gudang utama
            material.Jumlah += kuantitasPerubahan; // Nilai negatif akan otomatis mengurangi stok
            
            if (material.Jumlah < 0)
            {
                TempData["Error"] = "Transaksi ditolak. Koreksi menyebabkan angka stok akhir menjadi minus.";
                return RedirectToAction(nameof(Index));
            }

            _context.Update(material);

            // Suntik baris riwayat audit trail detail beserta lokasi Plant & SLOC
            var log = new HistoryMaterial
            {
                NoMaterial = material.NoMaterial,
                NamaUnit = material.NamaUnit,
                JenisAktivitas = jenisAktivitas,
                PerubahanJumlah = kuantitasPerubahan,
                SaldoAkhir = material.Jumlah,
                Plant = plant,
                StorageLocation = storageLocation,
                Keterangan = keterangan
            };
            _context.HistoryMaterial.Add(log);

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Audit log berhasil dicatat untuk aktivitas: {jenisAktivitas}.";
        }
        catch (Exception)
        {
            TempData["Error"] = "Gagal memproses penyesuaian logistik.";
        }

        return RedirectToAction(nameof(Index));
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
                material.Status = (material.Status == "Y") ? "N" : "Y";
                _context.Update(material);

                var log = new HistoryMaterial
                {
                    NoMaterial = material.NoMaterial,
                    NamaUnit = material.NamaUnit,
                    JenisAktivitas = material.Status == "Y" ? "Aktivasi Sistem" : "Deaktivasi Sistem",
                    PerubahanJumlah = 0,
                    SaldoAkhir = material.Jumlah,
                    Keterangan = $"Mengubah status penayangan opsi item di form input menjadi {(material.Status == "Y" ? "Aktif" : "Non-Aktif")}."
                };
                _context.HistoryMaterial.Add(log);

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Status {material.NamaUnit} berhasil diubah.";
            }
            catch (Exception) { TempData["Error"] = "Gagal memproses status."; }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TambahBarcodeBaru(string noMaterial, string barcodeBaru)
    {
        var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == noMaterial);
        if (material == null) return NotFound();

        // Cek apakah barcode sudah dipakai di barang lain
        var isExist = await _context.DataBarcode.AnyAsync(b => b.Barcode == barcodeBaru);
        if (isExist)
        {
            TempData["Error"] = $"GAGAL: Barcode '{barcodeBaru}' sudah terdaftar di sistem!";
            return RedirectToAction(nameof(Index));
        }

        // Cek agar jumlah barcode tidak melebihi stok fisik gudang
        var jumlahBarcodeSaatIni = await _context.DataBarcode.CountAsync(b => b.NoMaterial == noMaterial);
        if (jumlahBarcodeSaatIni >= material.Jumlah)
        {
            TempData["Error"] = $"GAGAL: Stok {material.NamaUnit} hanya ada {material.Jumlah}. Anda tidak bisa mendaftarkan barcode melebihi jumlah stok fisik.";
            return RedirectToAction(nameof(Index));
        }

        var newBarcode = new DataBarcode
        {
            NoMaterial = noMaterial,
            Barcode = barcodeBaru
        };

        _context.DataBarcode.Add(newBarcode);

        // Catat di History Log
        _context.HistoryMaterial.Add(new HistoryMaterial {
            NoMaterial = noMaterial, NamaUnit = material.NamaUnit,
            JenisAktivitas = "Registrasi Barcode Asset", PerubahanJumlah = 0, SaldoAkhir = material.Jumlah,
            Keterangan = $"Mendaftarkan barcode stiker fisik: {barcodeBaru}"
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Barcode {barcodeBaru} berhasil diikat ke material {material.NamaUnit}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> TambahPIC(MasterPIC pic)
    {
        if (ModelState.IsValid)
        {
            _context.MasterPIC.Add(pic);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> TambahLokasi(MasterLokasi lokasi)
    {
        if (ModelState.IsValid)
        {
            _context.MasterLokasi.Add(lokasi);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPIC(int id, string namaLengkap, string departemen)
    {
        var pic = await _context.MasterPIC.FindAsync(id);
        if (pic != null)
        {
            pic.NamaLengkap = namaLengkap;
            pic.Departemen = departemen;
            _context.Update(pic);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Data PIC berhasil diperbarui.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePIC(int id)
    {
        var pic = await _context.MasterPIC.FindAsync(id);
        if (pic != null)
        {
            try
            {
                _context.MasterPIC.Remove(pic);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Data PIC berhasil dihapus permanen.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Gagal: PIC ini tidak bisa dihapus karena namanya sudah tercatat di data Tugas Dinas.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLokasi(int id, string namaLokasi)
    {
        var lokasi = await _context.MasterLokasi.FindAsync(id);
        if (lokasi != null)
        {
            lokasi.NamaLokasi = namaLokasi;
            _context.Update(lokasi);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Data Lokasi berhasil diperbarui.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLokasi(int id)
    {
        var lokasi = await _context.MasterLokasi.FindAsync(id);
        if (lokasi != null)
        {
            try
            {
                _context.MasterLokasi.Remove(lokasi);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Data Lokasi berhasil dihapus.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Gagal: Lokasi ini tidak bisa dihapus karena sudah dipakai di transaksi.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMaterial(string noMaterialLama, string noMaterialBaru, string namaUnit, string satuan)
    {
        var material = await _context.MasterMaterial.FirstOrDefaultAsync(m => m.NoMaterial == noMaterialLama);
        if (material != null)
        {
            // Jika user mengubah Nomor Materialnya juga (Hati-hati, ini bisa memengaruhi relasi)
            if (noMaterialLama != noMaterialBaru)
            {
                var cekDuplicate = await _context.MasterMaterial.AnyAsync(m => m.NoMaterial == noMaterialBaru);
                if (cekDuplicate)
                {
                    TempData["Error"] = "Gagal Edit: Nomor Material baru sudah terpakai barang lain.";
                    return RedirectToAction(nameof(Index));
                }
            }

            material.NamaUnit = namaUnit;
            material.Satuan = satuan;
            _context.Update(material);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Detail material {namaUnit} berhasil diperbarui.";
        }
        return RedirectToAction(nameof(Index));
    }
}