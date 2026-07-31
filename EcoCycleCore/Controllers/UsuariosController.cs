using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoCycleCore.Models;

public class UsuariosController : Controller
{
    private readonly EcoCycleContext _context;

    public UsuariosController(EcoCycleContext context)
    {
        _context = context;
    }

    // GET: Usuarios
    public async Task<IActionResult> Index()
    {
        return View(await _context.Usuarios.ToListAsync());
    }

    // GET: Usuarios/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(m => m.UsuarioId == id);

        if (usuario == null) return NotFound();

        return View(usuario);
    }

    // GET: Usuarios/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Usuarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Correo,ContrasenaHash,Telefono,Direccion,TipoUsuario")] Usuario usuario)
    {
        if (ModelState.IsValid)
        {
            usuario.PuntosAcumulacion = 0;
            usuario.FechaRegistro = DateTime.Now;
            _context.Add(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(usuario);
    }

    // GET: Usuarios/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        return View(usuario);
    }

    // POST: Usuarios/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("UsuarioId,Nombre,Correo,ContrasenaHash,Telefono,Direccion,TipoUsuario,PuntosAcumulacion,FechaRegistro")] Usuario usuario)
    {
        if (id != usuario.UsuarioId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(usuario.UsuarioId))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(usuario);
    }

    // GET: Usuarios/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(m => m.UsuarioId == id);

        if (usuario == null) return NotFound();

        return View(usuario);
    }

    // POST: Usuarios/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Usuarios/Profile
    public async Task<IActionResult> Profile()
    {
        var sesionUsuario = HttpContext.Session.GetString("usuario");

        if (string.IsNullOrEmpty(sesionUsuario))
            return RedirectToAction("Login", "Auth");

        var usuario = await _context.Usuarios
            .Include(u => u.Publicaciones)
            .FirstOrDefaultAsync(m => m.Nombre == sesionUsuario);

        if (usuario == null) return NotFound();

        return View(usuario);
    }

    private bool UsuarioExists(int id)
    {
        return _context.Usuarios.Any(e => e.UsuarioId == id);
    }
}