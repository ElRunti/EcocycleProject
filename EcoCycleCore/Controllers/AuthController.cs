using BCrypt.Net;
using EcoCycleCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoCycleCore.Controllers
{
    public class AuthController : Controller
    {
        private readonly EcoCycleContext _context;

        public AuthController(EcoCycleContext context)
        {
            _context = context;
        }

        // ================= LOGIN =================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string password)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == correo);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario no encontrado";
                return View();
            }

            bool acceso = BCrypt.Net.BCrypt.Verify(
                password,
                usuario.ContrasenaHash
            );

            if (acceso)
            {
                // Nombre del usuario
                HttpContext.Session.SetString(
                    "usuario",
                    usuario.Nombre
                );

                // Rol
                HttpContext.Session.SetString(
                    "rol",
                    usuario.TipoUsuario
                );

                // ID del usuario (NUEVO)
                HttpContext.Session.SetInt32(
                    "usuarioId",
                    usuario.UsuarioId
                );

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            ViewBag.Error = "Contraseña incorrecta";
            return View();
        }

        // ================= REGISTER =================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(
            string nombre,
            string correo,
            string password,
            string tipo_usuario,
            string? documento_identidad
        )
        {
            var existe = _context.Usuarios
                .FirstOrDefault(u => u.Correo == correo);

            if (existe != null)
            {
                ViewBag.Error = "El usuario ya existe";
                return View();
            }

            Usuario usuario = new Usuario()
            {
                Nombre = nombre,
                Correo = correo,
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(password),
                TipoUsuario = tipo_usuario,
                DocumentoIdentidad = tipo_usuario == "Recolector"
                    ? documento_identidad
                    : null,
                FechaRegistro = DateTime.Now,
                PuntosAcumulacion = 0
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return RedirectToAction(nameof(Login));
        }

        // ================= LOGOUT =================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Index",
                "Home"
            );
        }
    }
}