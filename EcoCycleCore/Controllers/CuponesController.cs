using EcoCycleCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoCycleCore.Controllers
{
    public class CuponesController : Controller
    {
        private readonly EcoCycleContext _context;

        public CuponesController(EcoCycleContext context)
        {
            _context = context;
        }

        // GET: Cupones — catálogo público (todos pueden ver)
        public async Task<IActionResult> Index()
        {
            var cupones = await _context.Cupones
                .OrderBy(c => c.CostPuntos)
                .ToListAsync();

            // Si está logueado, pasar sus puntos para mostrar qué puede canjear
            var usuarioId = HttpContext.Session.GetInt32("usuarioId");
            if (usuarioId != null)
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                ViewBag.MisPuntos = usuario?.PuntosAcumulacion ?? 0;
            }
            else
            {
                ViewBag.MisPuntos = 0;
            }

            return View(cupones);
        }

        // GET: Cupones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cupone = await _context.Cupones
                .Include(c => c.Canjes)
                .FirstOrDefaultAsync(m => m.CuponId == id);

            if (cupone == null) return NotFound();

            return View(cupone);
        }

        // GET: Cupones/Create — solo admin
        public IActionResult Create()
        {
            var rol = HttpContext.Session.GetString("rol")?.ToLower();
            if (rol != "admin") return RedirectToAction("Login", "Auth");

            return View();
        }

        // POST: Cupones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TiendaNombre,DescBeneficio,CostPuntos,Stock")] Cupone cupone)
        {
            var rol = HttpContext.Session.GetString("rol")?.ToLower();
            if (rol != "admin") return Unauthorized();

            if (ModelState.IsValid)
            {
                _context.Add(cupone);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Cupón creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(cupone);
        }

        // GET: Cupones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var rol = HttpContext.Session.GetString("rol")?.ToLower();
            if (rol != "admin") return RedirectToAction("Login", "Auth");

            if (id == null) return NotFound();

            var cupone = await _context.Cupones.FindAsync(id);
            if (cupone == null) return NotFound();

            return View(cupone);
        }

        // POST: Cupones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CuponId,TiendaNombre,DescBeneficio,CostPuntos,Stock")] Cupone cupone)
        {
            var rol = HttpContext.Session.GetString("rol")?.ToLower();
            if (rol != "admin") return Unauthorized();

            if (id != cupone.CuponId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cupone);
                    await _context.SaveChangesAsync();
                    TempData["Ok"] = "Cupón actualizado.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cupones.Any(e => e.CuponId == cupone.CuponId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cupone);
        }

        // GET: Cupones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var rol = HttpContext.Session.GetString("rol")?.ToLower();
            if (rol != "admin") return RedirectToAction("Login", "Auth");

            if (id == null) return NotFound();

            var cupone = await _context.Cupones
                .FirstOrDefaultAsync(m => m.CuponId == id);

            if (cupone == null) return NotFound();

            return View(cupone);
        }

        // POST: Cupones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rol = HttpContext.Session.GetString("rol")?.ToLower();
            if (rol != "admin") return Unauthorized();

            var cupone = await _context.Cupones.FindAsync(id);
            if (cupone != null)
            {
                _context.Cupones.Remove(cupone);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Cupón eliminado.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cupones/Canjear/5 — solo usuarios logueados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Canjear(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("usuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var cupone = await _context.Cupones.FindAsync(id);
            if (cupone == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return NotFound();

            // Validar stock
            if (cupone.Stock <= 0)
            {
                TempData["Error"] = "Este cupón ya no tiene stock disponible.";
                return RedirectToAction(nameof(Index));
            }

            // Validar puntos suficientes
            if ((usuario.PuntosAcumulacion ?? 0) < cupone.CostPuntos)
            {
                TempData["Error"] = $"No tienes suficientes puntos. Necesitas {cupone.CostPuntos} pts y tienes {usuario.PuntosAcumulacion ?? 0} pts.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar que no lo haya canjeado ya (opcional, quita si quieres permitir múltiples)
            var yaCanjeado = await _context.Canjes
                .AnyAsync(c => c.UsuarioId == usuarioId && c.CuponId == id);

            if (yaCanjeado)
            {
                TempData["Error"] = "Ya canjeaste este cupón anteriormente.";
                return RedirectToAction(nameof(Index));
            }

            // Generar código único
            var codigo = $"ECO-{cupone.CuponId}-{usuarioId}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            // Crear el canje
            var canje = new Canje
            {
                CuponId = cupone.CuponId,
                UsuarioId = usuarioId.Value,
                CodigoDigital = codigo,
                FechaCanje = DateTime.Now
            };

            // Descontar puntos al usuario y stock al cupón
            usuario.PuntosAcumulacion = (usuario.PuntosAcumulacion ?? 0) - cupone.CostPuntos;
            cupone.Stock -= 1;

            _context.Canjes.Add(canje);
            await _context.SaveChangesAsync();

            TempData["Ok"] = $"¡Canje exitoso! Tu código es: {codigo}";
            return RedirectToAction("MisCanjes", "Canjes");
        }
    }
}