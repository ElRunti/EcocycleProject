
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

    // GET: USUARIOS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Usuarios.ToListAsync());
    }

    // GET: USUARIOS/Details/5
    public async Task<IActionResult> Details(int? usuarioid)
    {
        if (usuarioid == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(m => m.UsuarioId == usuarioid);
        if (usuario == null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    // GET: USUARIOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: USUARIOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UsuarioId,Nombre,Correo,ContrasenaHash,Telefono,Direccion,TipoUsuario,PuntosAcumulacion,FechaRegistro,Canjes,Entregas,Publicaciones")] Usuario usuario)
    {
        if (ModelState.IsValid)
        {
            _context.Add(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(usuario);
    }

    // GET: USUARIOS/Edit/5
    public async Task<IActionResult> Edit(int? usuarioid)
    {
        if (usuarioid == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios.FindAsync(usuarioid);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    // POST: USUARIOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? usuarioid, [Bind("UsuarioId,Nombre,Correo,ContrasenaHash,Telefono,Direccion,TipoUsuario,PuntosAcumulacion,FechaRegistro,Canjes,Entregas,Publicaciones")] Usuario usuario)
    {
        if (usuarioid != usuario.UsuarioId)
        {
            return NotFound();
        }

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
        return View(usuario);
    }

    // GET: USUARIOS/Delete/5
    public async Task<IActionResult> Delete(int? usuarioid)
    {
        if (usuarioid == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(m => m.UsuarioId == usuarioid);
        if (usuario == null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    // POST: USUARIOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? usuarioid)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioid);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UsuarioExists(int? usuarioid)
    {
        return _context.Usuarios.Any(e => e.UsuarioId == usuarioid);
    }
    // GET: USUARIOS/Profile
    public async Task<IActionResult> Profile()
    {
        // 1. Obtenemos el valor de la sesión (nombre o correo)
        var sesionUsuario = HttpContext.Session.GetString("usuario");

        // 2. Verificamos si hay una sesión activa
        if (string.IsNullOrEmpty(sesionUsuario))
        {
            // Si no está logueado, lo mandamos al Login
            return RedirectToAction("Login", "Auth");
        }

        // 3. Buscamos al usuario en la base de datos usando el valor de la sesión.
        // NOTA: Si en tu sesión guardas el 'Correo' en lugar del 'Nombre', cambia m.Nombre por m.Correo
        var usuario = await _context.Usuarios
            .Include(u => u.Publicaciones) // Trae la lista de publicaciones del usuario
            .FirstOrDefaultAsync(m => m.Nombre == sesionUsuario);

        if (usuario == null)
        {
            return NotFound();
        }

        return View(usuario);
    }
}
