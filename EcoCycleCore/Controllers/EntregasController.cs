using EcoCycleCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class EntregasController : Controller
{
    private readonly EcoCycleContext _context;

    public EntregasController(EcoCycleContext context)
    {
        _context = context;
    }

    // GET: ENTREGAS
    public async Task<IActionResult> Index()
    {
        var entregas = await _context.Entregas
            .Include(e => e.Ciudadano)
            .Include(e => e.Publicacion)
            .ToListAsync();

        return View(entregas);
    }

    // GET: ENTREGAS/Details/5
    public async Task<IActionResult> Details(int? entregaid)
    {
        if (entregaid == null)
        {
            return NotFound();
        }

        var entrega = await _context.Entregas
            .Include(e => e.Ciudadano)
            .Include(e => e.Publicacion)
            .FirstOrDefaultAsync(m => m.EntregaId == entregaid);

        if (entrega == null)
        {
            return NotFound();
        }

        return View(entrega);
    }

    // GET: ENTREGAS/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.PublicacionId = new SelectList(
            await _context.Publicaciones
                .Include(p => p.Material)
                .Include(p => p.Usuario)
                .Where(p => p.Estado == "Pendiente") // ajusta según tus valores reales de Estado
                .ToListAsync(),
            "PublicacionesId", "Descripcion");

        return View();
    }

    // POST: ENTREGAS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PublicacionId,CentroId,PesoReal")] Entrega entrega)
    {
        var publicacion = await _context.Publicaciones
            .Include(p => p.Material)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.PublicacionesId == entrega.PublicacionId);

        if (publicacion == null)
        {
            ModelState.AddModelError("", "La publicación seleccionada no existe.");
            return View(entrega);
        }

        if (ModelState.IsValid)
        {
            entrega.CiudadanoId = publicacion.UsuarioId;

            decimal puntosCalculados = entrega.PesoReal * publicacion.Material.PuntosPorKilo;
            entrega.PuntosOtorgados = (int)puntosCalculados;

            entrega.FechaEntrega = DateTime.Now;

            publicacion.Usuario.PuntosAcumulacion =
                (publicacion.Usuario.PuntosAcumulacion ?? 0) + entrega.PuntosOtorgados;

            publicacion.Estado = "Completada"; // ajusta según tus valores reales

            _context.Entregas.Add(entrega);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(entrega);
    }

    // GET: ENTREGAS/Edit/5
    public async Task<IActionResult> Edit(int? entregaid)
    {
        if (entregaid == null)
        {
            return NotFound();
        }

        var entrega = await _context.Entregas
            .Include(e => e.Ciudadano)
            .Include(e => e.Publicacion)
            .FirstOrDefaultAsync(m => m.EntregaId == entregaid);

        if (entrega == null)
        {
            return NotFound();
        }
        return View(entrega);
    }

    // POST: ENTREGAS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? entregaid, [Bind("EntregaId,PublicacionId,CiudadanoId,CentroId,PesoReal,PuntosOtorgados,FechaEntrega")] Entrega entrega)
    {
        if (entregaid != entrega.EntregaId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(entrega);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntregaExists(entrega.EntregaId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(entrega);
    }

    // GET: ENTREGAS/Delete/5
    public async Task<IActionResult> Delete(int? entregaid)
    {
        if (entregaid == null)
        {
            return NotFound();
        }

        var entrega = await _context.Entregas
            .Include(e => e.Ciudadano)
            .Include(e => e.Publicacion)
            .FirstOrDefaultAsync(m => m.EntregaId == entregaid);

        if (entrega == null)
        {
            return NotFound();
        }

        return View(entrega);
    }

    // POST: ENTREGAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? entregaid)
    {
        var entrega = await _context.Entregas.FindAsync(entregaid);
        if (entrega != null)
        {
            _context.Entregas.Remove(entrega);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool EntregaExists(int? entregaid)
    {
        return _context.Entregas.Any(e => e.EntregaId == entregaid);
    }
}