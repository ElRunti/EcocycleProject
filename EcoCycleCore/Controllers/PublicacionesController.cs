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

        // GET: Publicaciones
        // GET: Publicaciones
        public async Task<IActionResult> Index()
        {
            string? rol = HttpContext.Session.GetString("rol");
            int? usuarioId = HttpContext.Session.GetInt32("usuarioId");

            var publicaciones = _context.Publicaciones
                .Include(p => p.Material)
                .Include(p => p.Usuario)
                .Include(p => p.Recolector)
                .AsQueryable();

            // ADMIN
            if (rol == "Admin")
            {
                return View(await publicaciones.ToListAsync());
            }

            // USUARIO
            if (rol == "Usuario")
            {
                publicaciones = publicaciones.Where(p => p.UsuarioId == usuarioId);

                return View(await publicaciones.ToListAsync());
            }

            // RECOLECTOR
            if (rol == "Recolector")
            {
                publicaciones = publicaciones.Where(p =>
                    p.Estado == "Pendiente"
                    || (p.RecolectorId == usuarioId && p.Estado != "Finalizada"));

                return View(await publicaciones.ToListAsync());
            }

            return RedirectToAction("Login", "Auth");
        }

        // GET: Publicaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicacione = await _context.Publicaciones
                .Include(p => p.Material)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.PublicacionesId == id);
            if (publicacione == null)
            {
                return NotFound();
            }

            return View(publicacione);
        }

        // GET: Publicaciones/Create
        public IActionResult Create()
        {
            ViewData["MaterialId"] = new SelectList(
                _context.Materiales.OrderBy(m => m.NombreMaterial),
                "MaterialId",
                "NombreMaterial");

            return View();
        }

        // POST: Publicaciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publicacione publicacione)
        {
            var usuarioId = HttpContext.Session.GetInt32("usuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Quitamos validaciones de propiedades que NO vienen del formulario
            ModelState.Remove("Estado");
            ModelState.Remove("FechaPublicacion");
            ModelState.Remove("Usuario");
            ModelState.Remove("Material");
            ModelState.Remove("Recolector");

            // Asignamos los valores automáticamente
            publicacione.UsuarioId = usuarioId.Value;
            publicacione.Estado = "Pendiente";
            publicacione.FechaPublicacion = DateTime.Now;
            publicacione.RecolectorId = null;

            if (!ModelState.IsValid)
            {
                ViewData["MaterialId"] = new SelectList(
                    _context.Materiales,
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
            if (id == null)
            {
                return NotFound();
            }

            var publicacione = await _context.Publicaciones.FindAsync(id);
            if (publicacione == null)
            {
                return NotFound();
            }
            ViewData["MaterialId"] = new SelectList(_context.Materiales, "MaterialId", "MaterialId", publicacione.MaterialId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", publicacione.UsuarioId);
            return View(publicacione);
        }

        // POST: Publicaciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PublicacionesId,UsuarioId,MaterialId,Descripcion,PesoCantidad,Ubicacion,UrlImagen,Estado,FechaPublicacion")] Publicacione publicacione)
        {
            if (id != publicacione.PublicacionesId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publicacione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublicacioneExists(publicacione.PublicacionesId))
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
            ViewData["MaterialId"] = new SelectList(_context.Materiales, "MaterialId", "NombreMaterial", publicacione.MaterialId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", publicacione.UsuarioId);
            return View(publicacione);
        }

        // GET: Publicaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicacione = await _context.Publicaciones
                .Include(p => p.Material)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.PublicacionesId == id);
            if (publicacione == null)
            {
                return NotFound();
            }

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

        private bool PublicacioneExists(int id)
        {
            return _context.Publicaciones.Any(e => e.PublicacionesId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarRecoleccion(int id)
        {
            string? rol = HttpContext.Session.GetString("rol");

            if (rol != "Recolector")
            {
                return Unauthorized();
            }

            int? recolectorId = HttpContext.Session.GetInt32("usuarioId");

            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.PublicacionesId == id);

            if (publicacion == null)
            {
                return NotFound();
            }

            // Ya fue tomada por otro recolector
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AceptarSolicitud(int id)
        {
            if (HttpContext.Session.GetString("rol") != "Usuario")
                return Unauthorized();

            int usuarioId = HttpContext.Session.GetInt32("usuarioId").Value;

            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.PublicacionesId == id);

            if (publicacion == null)
                return NotFound();

            if (publicacion.UsuarioId != usuarioId)
                return Unauthorized();

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarSolicitud(int id)
        {
            if (HttpContext.Session.GetString("rol") != "Usuario")
                return Unauthorized();

            int usuarioId = HttpContext.Session.GetInt32("usuarioId").Value;

            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.PublicacionesId == id);

            if (publicacion == null)
                return NotFound();

            if (publicacion.UsuarioId != usuarioId)
                return Unauthorized();

            publicacion.RecolectorId = null;
            publicacion.Estado = "Pendiente";

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Solicitud rechazada.";

            return RedirectToAction(nameof(Index));
        }

    }
}
