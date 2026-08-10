using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcoCycleCore.Models;

namespace EcoCycleCore.Controllers
{
    public class PublicacionesController : Controller
    {
        private readonly EcoCycleContext _context;

        public PublicacionesController(EcoCycleContext context)
        {
            _context = context;
        }

        // ==========================================
        // LISTADO PRINCIPAL (INDEX)
        // ==========================================

        // GET: Publicaciones
        public async Task<IActionResult> Index()
        {
            string? rol = HttpContext.Session.GetString("rol")?.ToLower();
            int? usuarioId = HttpContext.Session.GetInt32("usuarioId");

            if (string.IsNullOrEmpty(rol) || usuarioId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var publicaciones = _context.Publicaciones
                .Include(p => p.Material)
                .Include(p => p.Usuario)
                .Include(p => p.Recolector)
                .AsQueryable();

            // ADMIN
            if (rol == "admin")
            {
                return View(await publicaciones.ToListAsync());
            }

            // USUARIO
            if (rol == "usuario")
            {
                publicaciones = publicaciones.Where(p => p.UsuarioId == usuarioId.Value);
                return View(await publicaciones.ToListAsync());
            }

            // RECOLECTOR
            if (rol == "recolector")
            {
                publicaciones = publicaciones.Where(p =>
                    p.Estado == "Pendiente"
                    || (p.RecolectorId == usuarioId.Value && p.Estado != "Finalizada"));

                return View(await publicaciones.ToListAsync());
            }

            return RedirectToAction("Login", "Auth");
        }

        // ==========================================
        // GESTIÓN DE EVIDENCIAS Y REVISIÓN ADMIN
        // ==========================================

        // GET: Recolector abre formulario para subir evidencia
        [HttpGet]
        public async Task<IActionResult> SubirEvidencia(int id)
        {
            string? rol = HttpContext.Session.GetString("rol")?.ToLower();
            int? recolectorId = HttpContext.Session.GetInt32("usuarioId");

            if (rol != "recolector" || recolectorId == null)
                return RedirectToAction("Login", "Auth");

            var publicacion = await _context.Publicaciones
                .Include(p => p.Material)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.PublicacionesId == id);

            if (publicacion == null) return NotFound();
            if (publicacion.RecolectorId != recolectorId.Value) return Unauthorized();

            return View(publicacion);
        }

        // POST: Recolector envía evidencia y marca como "En Revision"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirEvidencia(int id, string evidenciaUrl,
    bool entregaCorrecta, string? notaRecolector)
        {
            string? rol = HttpContext.Session.GetString("rol")?.ToLower();
            int? recolectorId = HttpContext.Session.GetInt32("usuarioId");

            if (rol != "recolector" || recolectorId == null)
                return RedirectToAction("Login", "Auth");

            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return NotFound();
            if (publicacion.RecolectorId != recolectorId.Value) return Unauthorized();

            if (string.IsNullOrWhiteSpace(evidenciaUrl))
            {
                TempData["Error"] = "Debes ingresar una URL de evidencia.";
                return RedirectToAction(nameof(SubirEvidencia), new { id });
            }

            if (!entregaCorrecta && string.IsNullOrWhiteSpace(notaRecolector))
            {
                TempData["Error"] = "Debes describir el problema en la nota.";
                return RedirectToAction(nameof(SubirEvidencia), new { id });
            }

            publicacion.EvidenciaUrl = evidenciaUrl;
            publicacion.EntregaCorrecta = entregaCorrecta;
            publicacion.NotaRecolector = notaRecolector;
            publicacion.Estado = "En Revision";

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Evidencia enviada correctamente. El administrador la revisará pronto.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin aprueba la entrega y deposita puntos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobarEntregaAdmin(int id, int puntosOtorgados)
        {
            string? rol = HttpContext.Session.GetString("rol")?.ToLower();
            if (rol != "admin") return Unauthorized();

            var publicacion = await _context.Publicaciones
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.PublicacionesId == id);

            if (publicacion == null) return NotFound();

            publicacion.Estado = "Finalizada";

            if (publicacion.Usuario != null)
            {
                publicacion.Usuario.PuntosAcumulacion =
                    (publicacion.Usuario.PuntosAcumulacion ?? 0) + puntosOtorgados;
            }

            await _context.SaveChangesAsync();

            TempData["Ok"] = $"Entrega aprobada. Se depositaron {puntosOtorgados} puntos al usuario.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin rechaza la evidencia y regresa a Aceptada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarEvidencia(int id)
        {
            string? rol = HttpContext.Session.GetString("rol")?.ToLower();
            if (rol != "admin") return Unauthorized();

            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return NotFound();

            publicacion.Estado = "Aceptada";
            publicacion.EvidenciaUrl = null;

            await _context.SaveChangesAsync();

            TempData["Error"] = "Evidencia rechazada. El recolector deberá subir nueva evidencia.";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // FLUJO DE SOLICITUDES Y RECOLECCIÓN
        // ==========================================

        // POST: Recolector solicita tomar una publicación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarRecoleccion(int id)
        {
            string? rol = HttpContext.Session.GetString("rol")?.ToLower();
            int? recolectorId = HttpContext.Session.GetInt32("usuarioId");

            if (rol != "recolector" || recolectorId == null) return Unauthorized();

            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return NotFound();

            if (publicacion.RecolectorId != null)
            {
                TempData["Error"] = "Esta publicación ya fue tomada por otro recolector.";
                return RedirectToAction(nameof(Index));
            }

            publicacion.RecolectorId = recolectorId;
            publicacion.Estado = "Solicitada";

            await _context.SaveChangesAsync();
            TempData["Ok"] = "Has solicitado esta recolección.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Usuario acepta al recolector que solicitó la recolección
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AceptarSolicitud(int id)
        {
            string? rol = HttpContext.Session.GetString("rol")?.ToLower();
            int? usuarioId = HttpContext.Session.GetInt32("usuarioId");

            if (rol != "usuario" || usuarioId == null) return Unauthorized();

            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return NotFound();

            if (publicacion.UsuarioId != usuarioId.Value) return Unauthorized();

            if (publicacion.RecolectorId == null)
            {
                TempData["Error"] = "La publicación no tiene un recolector asignado.";
                return RedirectToAction(nameof(Index));
            }

            publicacion.Estado = "Aceptada";
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Has aceptado al recolector.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Usuario rechaza la solicitud del recolector
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarSolicitud(int id)
        {
            string? rol = HttpContext.Session.GetString("rol")?.ToLower();
            int? usuarioId = HttpContext.Session.GetInt32("usuarioId");

            if (rol != "usuario" || usuarioId == null) return Unauthorized();

            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return NotFound();

            if (publicacion.UsuarioId != usuarioId.Value) return Unauthorized();

            publicacion.RecolectorId = null;
            publicacion.Estado = "Pendiente";

            await _context.SaveChangesAsync();
            TempData["Ok"] = "Solicitud rechazada.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Recolector cancela su solicitud de recolección
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarSolicitud(int id)
        {
            string? rol = HttpContext.Session.GetString("rol")?.ToLower();
            int? recolectorId = HttpContext.Session.GetInt32("usuarioId");

            if (rol != "recolector" || recolectorId == null) return Unauthorized();

            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return NotFound();

            if (publicacion.RecolectorId == recolectorId.Value && publicacion.Estado == "Solicitada")
            {
                publicacion.RecolectorId = null;
                publicacion.Estado = "Pendiente";
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Has cancelado tu solicitud de recolección.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CRUD BÁSICO (DETAILS, CREATE, EDIT, DELETE)
        // ==========================================

        // GET: Publicaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var publicacione = await _context.Publicaciones
                .Include(p => p.Material)
                .Include(p => p.Usuario)
                .Include(p => p.Recolector)
                .FirstOrDefaultAsync(m => m.PublicacionesId == id);

            if (publicacione == null) return NotFound();

            return View(publicacione);
        }

        // GET: Publicaciones/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("usuarioId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewData["MaterialId"] = new SelectList(
                _context.Materiales.OrderBy(m => m.NombreMaterial),
                "MaterialId",
                "NombreMaterial");

            return View();
        }

        // POST: Publicaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publicacione publicacione)
        {
            var usuarioId = HttpContext.Session.GetInt32("usuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            ModelState.Remove("Estado");
            ModelState.Remove("FechaPublicacion");
            ModelState.Remove("Usuario");
            ModelState.Remove("Material");
            ModelState.Remove("Recolector");

            publicacione.UsuarioId = usuarioId.Value;
            publicacione.Estado = "Pendiente";
            publicacione.FechaPublicacion = DateTime.Now;
            publicacione.RecolectorId = null;

            if (!ModelState.IsValid)
            {
                ViewData["MaterialId"] = new SelectList(
                    _context.Materiales.OrderBy(m => m.NombreMaterial),
                    "MaterialId",
                    "NombreMaterial",
                    publicacione.MaterialId);

                return View(publicacione);
            }

            _context.Publicaciones.Add(publicacione);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "La publicación fue creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Publicaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var publicacione = await _context.Publicaciones.FindAsync(id);
            if (publicacione == null) return NotFound();

            ViewData["MaterialId"] = new SelectList(_context.Materiales, "MaterialId", "NombreMaterial", publicacione.MaterialId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "Nombre", publicacione.UsuarioId);
            return View(publicacione);
        }

        // POST: Publicaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PublicacionesId,UsuarioId,MaterialId,Descripcion,PesoCantidad,Ubicacion,UrlImagen,Estado,FechaPublicacion")] Publicacione publicacione)
        {
            if (id != publicacione.PublicacionesId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publicacione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublicacioneExists(publicacione.PublicacionesId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaterialId"] = new SelectList(_context.Materiales, "MaterialId", "NombreMaterial", publicacione.MaterialId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "Nombre", publicacione.UsuarioId);
            return View(publicacione);
        }

        // GET: Publicaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var publicacione = await _context.Publicaciones
                .Include(p => p.Material)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.PublicacionesId == id);

            if (publicacione == null) return NotFound();

            return View(publicacione);
        }

        // POST: Publicaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publicacione = await _context.Publicaciones.FindAsync(id);
            if (publicacione != null)
            {
                _context.Publicaciones.Remove(publicacione);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ==========================================

        private bool PublicacioneExists(int id)
        {
            return _context.Publicaciones.Any(e => e.PublicacionesId == id);
        }
    }
}