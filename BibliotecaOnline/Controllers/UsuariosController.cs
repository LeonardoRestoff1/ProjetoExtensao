using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaOnline.Data;
using BibliotecaOnline.Infrastructure;
using BibliotecaOnline.Models;

namespace BibliotecaOnline.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public UsuariosController(BibliotecaDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() =>
            HttpContext.Session.GetString(SessionAuth.AuthRoleKey) == SessionAuth.RoleAdmin;

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            ViewData["NavSection"] = "usuarios";
            return View(await _context.Usuarios.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Emprestimos)
                    .ThenInclude(e => e.Livro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            ViewData["NavSection"] = "usuarios";
            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            ViewData["NavSection"] = "usuarios";
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Email,Telefone,Senha")] Usuario usuario)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (string.IsNullOrWhiteSpace(usuario.Senha) || usuario.Senha.Length < 4)
            {
                ModelState.AddModelError(nameof(usuario.Senha), "Informe uma senha com pelo menos 4 caracteres.");
            }

            if (ModelState.IsValid)
            {
                usuario.Nome = usuario.Nome.Trim();
                usuario.Email = usuario.Email.Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(usuario.Telefone))
                    usuario.Telefone = usuario.Telefone.Trim();
                usuario.Senha = usuario.Senha.Trim();
                usuario.Id = Guid.NewGuid().ToString();
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Usuário cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Senha = string.Empty;
            ViewData["NavSection"] = "usuarios";
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nome,Email,Telefone,Senha")] Usuario usuario)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id != usuario.Id)
            {
                return NotFound();
            }

            var existing = await _context.Usuarios.FindAsync(id);
            if (existing == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                existing.Nome = usuario.Nome.Trim();
                existing.Email = usuario.Email.Trim().ToLowerInvariant();
                existing.Telefone = string.IsNullOrWhiteSpace(usuario.Telefone) ? null : usuario.Telefone.Trim();
                if (!string.IsNullOrWhiteSpace(usuario.Senha))
                    existing.Senha = usuario.Senha;

                try
                {
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Usuário atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            usuario.Senha = string.Empty;
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            ViewData["NavSection"] = "usuarios";
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return RedirectToAction(nameof(Index));

            var temEmprestimos = await _context.Emprestimos.AnyAsync(e => e.IdUsuario == id);
            if (temEmprestimos)
            {
                TempData["ErrorMessage"] = "Não é possível excluir este usuário pois existem empréstimos vinculados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Usuário excluído com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Emprestimos/5
        public async Task<IActionResult> Emprestimos(string id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Emprestimos)
                    .ThenInclude(e => e.Livro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            var emprestimosAtivos = usuario.Emprestimos?
                .Where(e => e.DataDevolucaoReal == null && !e.AguardandoAprovacao)
                .ToList() ?? new List<Emprestimo>();

            ViewBag.Usuario = usuario;
            ViewData["NavSection"] = "usuarios";
            return View(emprestimosAtivos);
        }

        private bool UsuarioExists(string id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
