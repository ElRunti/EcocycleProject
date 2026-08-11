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

    // GET: Entregas
    public async Task<IActionResult> Index()
    {
        var entregas = await _context.Entregas
            .Include(e => e.Ciudadano)
            .Include(e => e.Publicacion)
                .ThenInclude(p => p.Recolector)
            .ToListAsync();

        return View(entregas);
    }

    // GET: Entregas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var entrega = await _context.Entregas
            .Include(e => e.Ciudadano)
            .Include(e => e.Publicacion)
                .ThenInclude(p => p.Recolector)
            .FirstOrDefaultAsync(m => m.EntregaId == id);

        if (entrega == null) return NotFound();

        return View(entrega);
    }

    // GET: Entregas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var entrega = await _context.Entregas
            .Include(e => e.Ciudadano)
            .Include(e => e.Publicacion)
                .ThenInclude(p => p.Recolector)
            .FirstOrDefaultAsync(m => m.EntregaId == id);

        if (entrega == null) return NotFound();

        return View(entrega);
    }
    // GET: ENTREGAS/Details/5


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
    // GET: Entregas/Pendientes — admin ve las que están En Revision
    public async Task<IActionResult> Pendientes()
    {
        string? rol = HttpContext.Session.GetString("rol")?.ToLower();
        if (rol != "admin") return RedirectToAction("Login", "Auth");

        var pendientes = await _context.Publicaciones
            .Include(p => p.Material)
            .Include(p => p.Usuario)
            .Include(p => p.Recolector)
            .Where(p => p.Estado == "En Revision")
            .ToListAsync();

        return View(pendientes);
    }

    // POST: Admin aprueba desde el módulo Entregas
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AprobarEntrega(int publicacionId, int? puntosAjustados)
    {
        string? rol = HttpContext.Session.GetString("rol")?.ToLower();
        if (rol != "admin") return Unauthorized();

        var publicacion = await _context.Publicaciones
            .Include(p => p.Material)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.PublicacionesId == publicacionId);

        if (publicacion == null) return NotFound();

        // Calcular puntos automáticamente
        int puntosFinales;

        if (publicacion.EntregaCorrecta == true)
        {
            // Entrega sin problemas: cálculo automático
            puntosFinales = (int)(publicacion.PesoCantidad * (publicacion.Material?.PuntosPorKilo ?? 0));
        }
        else
        {
            // Hubo problema: admin ajusta manualmente
            if (puntosAjustados == null || puntosAjustados < 0)
            {
                TempData["Error"] = "Debes ingresar los puntos a otorgar manualmente.";
                return RedirectToAction(nameof(Pendientes));
            }
            puntosFinales = puntosAjustados.Value;
        }

        // Crear la entrega
        var entrega = new Entrega
        {
            PublicacionId = publicacion.PublicacionesId,
            CiudadanoId = publicacion.UsuarioId,
            CentroId = 1, // ajusta según tu lógica de centro
            PesoReal = publicacion.PesoCantidad,
            PuntosOtorgados = puntosFinales,
            FechaEntrega = DateTime.Now
        };

        _context.Entregas.Add(entrega);

        // Sumar puntos al usuario
        if (publicacion.Usuario != null)
            publicacion.Usuario.PuntosAcumulacion =
                (publicacion.Usuario.PuntosAcumulacion ?? 0) + puntosFinales;

        publicacion.Estado = "Finalizada";

        await _context.SaveChangesAsync();

        TempData["Ok"] = $"Entrega aprobada. Se otorgaron {puntosFinales} puntos al usuario {publicacion.Usuario?.Nombre}.";
        return RedirectToAction(nameof(Pendientes));
    }

    // POST: Admin rechaza evidencia
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RechazarEvidencia(int publicacionId)
    {
        string? rol = HttpContext.Session.GetString("rol")?.ToLower();
        if (rol != "admin") return Unauthorized();

        var publicacion = await _context.Publicaciones.FindAsync(publicacionId);
        if (publicacion == null) return NotFound();

        publicacion.Estado = "Aceptada";
        publicacion.EvidenciaUrl = null;
        publicacion.NotaRecolector = null;
        publicacion.EntregaCorrecta = null;

        await _context.SaveChangesAsync();

        TempData["Error"] = "Evidencia rechazada. El recolector deberá subir nueva evidencia.";
        return RedirectToAction(nameof(Pendientes));
    }
}