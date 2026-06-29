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
    public class MaterialesController : Controller
    {
        private readonly EcoCycleContext _context;

        public MaterialesController(EcoCycleContext context)
        {
            _context = context;
        }

        // GET: Materiales
        public async Task<IActionResult> Index()
        {
            return View(await _context.Materiales.ToListAsync());
        }

        // GET: Materiales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materiale = await _context.Materiales
                .FirstOrDefaultAsync(m => m.MaterialId == id);
            if (materiale == null)
            {
                return NotFound();
            }

            return View(materiale);
        }

        // GET: Materiales/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Materiales/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaterialId,NombreMaterial,PuntosPorKilo")] Materiale materiale)
        {
            if (ModelState.IsValid)
            {
                _context.Add(materiale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(materiale);
        }

        // GET: Materiales/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materiale = await _context.Materiales.FindAsync(id);
            if (materiale == null)
            {
                return NotFound();
            }
            return View(materiale);
        }

        // POST: Materiales/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaterialId,NombreMaterial,PuntosPorKilo")] Materiale materiale)
        {
            if (id != materiale.MaterialId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(materiale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialeExists(materiale.MaterialId))
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
            return View(materiale);
        }

        // GET: Materiales/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materiale = await _context.Materiales
                .FirstOrDefaultAsync(m => m.MaterialId == id);
            if (materiale == null)
            {
                return NotFound();
            }

            return View(materiale);
        }

        // POST: Materiales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var materiale = await _context.Materiales.FindAsync(id);
            if (materiale != null)
            {
                _context.Materiales.Remove(materiale);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialeExists(int id)
        {
            return _context.Materiales.Any(e => e.MaterialId == id);
        }
    }
}
