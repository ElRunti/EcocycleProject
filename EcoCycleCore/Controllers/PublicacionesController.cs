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
        public async Task<IActionResult> Index()
        {
            var ecoCycleContext = _context.Publicaciones.Include(p => p.Material).Include(p => p.Usuario);
            return View(await ecoCycleContext.ToListAsync());
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
            ViewData["MaterialId"] = new SelectList(_context.Materiales, "MaterialId", "MaterialId");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId");
            return View();
        }

        // POST: Publicaciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PublicacionesId,UsuarioId,MaterialId,Descripcion,PesoCantidad,Ubicacion,UrlImagen,Estado,FechaPublicacion")] Publicacione publicacione)
        {
            if (ModelState.IsValid)
            {
                _context.Add(publicacione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaterialId"] = new SelectList(_context.Materiales, "MaterialId", "NombreMaterial", publicacione.MaterialId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", publicacione.UsuarioId);
            return View(publicacione);
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
            ViewData["MaterialId"] = new SelectList(_context.Materiales, "MaterialId", "MaterialId", publicacione.MaterialId);
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
    }
}
