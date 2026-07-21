using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangKeluarDinas.Data;
using BarangKeluarDinas.Models;

namespace BarangKeluarDinas.Controllers;

public class RequestMaterialController : Controller
{
    private readonly AppDbContext _context;

    public RequestMaterialController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _context.RequestMaterial.OrderByDescending(r => r.TanggalRequest).ToListAsync();
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.MaterialList = await _context.MasterMaterial.Where(m => m.Status == "Y").ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RequestMaterial request)
    {
        if (ModelState.IsValid)
        {
            request.Id = Guid.NewGuid();
            request.TanggalRequest = DateTime.Now;
            request.Status = "Pending"; 

            _context.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Permintaan pengadaan material berhasil dikirim!";
            return RedirectToAction(nameof(Index));
        }


        ViewBag.MaterialList = await _context.MasterMaterial.Where(m => m.Status == "Y").ToListAsync();
        return View(request);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var request = await _context.RequestMaterial.FindAsync(id);
        if (request == null) return NotFound();

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProsesRequest(Guid id, string statusKeputusan, string catatanAdmin)
    {
        var request = await _context.RequestMaterial.FindAsync(id);
        if (request == null) return NotFound();

        request.Status = statusKeputusan;
        request.CatatanAdmin = catatanAdmin;

        _context.Update(request);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Pengajuan dari {request.NamaPemohon} telah ditandai sebagai: {statusKeputusan}.";
        return RedirectToAction(nameof(Index));
    }
}