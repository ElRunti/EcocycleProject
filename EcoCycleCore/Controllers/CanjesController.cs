using EcoCycleCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoCycleCore.Controllers
{
    public class CanjesController : Controller
    {
        private readonly EcoCycleContext _context;

        public CanjesController(EcoCycleContext context)
        {
            _context = context;
        }

        // GET: Canjes — admin ve todos los canjes
        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("rol")?.ToLower();
            if (rol != "admin") return RedirectToAction("Login", "Auth");

            var canjes = await _context.Canjes
                .Include(c => c.Cupon)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaCanje)
                .ToListAsync();

            return View(canjes);
        }

        // GET: Canjes/MisCanjes — usuario ve los suyos
        public async Task<IActionResult> MisCanjes()
        {
            var usuarioId = HttpContext.Session.GetInt32("usuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var canjes = await _context.Canjes
                .Include(c => c.Cupon)
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.FechaCanje)
                .ToListAsync();

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            ViewBag.MisPuntos = usuario?.PuntosAcumulacion ?? 0;

            return View(canjes);
        }
    }
}