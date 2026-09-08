using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaOnline.Data;
using BibliotecaOnline.Infrastructure;
using BibliotecaOnline.Models;

namespace BibliotecaOnline.Controllers
{
    public class LivrosController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public LivrosController(BibliotecaDbContext context)
        {
            _context = context;
        }

        // GET: Livros — Consulta de acervo (protótipo)
        public async Task<IActionResult> Index(string? q, string? categoria, bool? disponiveis)
        {
            ViewData["NavSection"] = "acervo";

            var query = _context.Livros.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                query = query.Where(l =>
                    (l.Titulo != null && l.Titulo.Contains(t)) ||
                    (l.Autor != null && l.Autor.Contains(t)) ||
                    (l.Categoria != null && l.Categoria.Contains(t)));
            }

            if (!string.IsNullOrWhiteSpace(categoria) && categoria != "Todos")
            {
                query = query.Where(l => l.Categoria == categoria);
            }

            if (disponiveis == true)
            {
                query = query.Where(l => l.Quantidade > 0);
            }

            ViewBag.Query = q;
            ViewBag.Categoria = categoria ?? "Todos";
            ViewBag.Disponiveis = disponiveis;
            ViewBag.IsAdmin = HttpContext.Session.GetString(SessionAuth.AuthRoleKey) == SessionAuth.RoleAdmin;
            ViewBag.Categorias = await _context.Livros
                .Where(l => l.Categoria != null && l.Categoria != "")
                .Select(l => l.Categoria!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var lista = await query.OrderBy(l => l.Titulo).ToListAsync();
            return View(lista);
        }

        // GET: Livros/Details/5
        public async Task<IActionResult> Details(string id)
        {
            ViewData["NavSection"] = "acervo";
            if (id == null)
            {
                return NotFound();
            }

            var livro = await _context.Livros
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (livro == null)
            {
                return NotFound();
            }

            ViewBag.IsAdmin = HttpContext.Session.GetString(SessionAuth.AuthRoleKey) == SessionAuth.RoleAdmin;
            ViewBag.LoggedUser = HttpContext.Session.GetString(SessionAuth.AuthRoleKey) == SessionAuth.RoleUser;
            return View(livro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Solicitar(string id)
        {
            if (HttpContext.Session.GetString(SessionAuth.AuthRoleKey) != SessionAuth.RoleUser)
            {
                TempData["ErrorMessage"] = "Faça login como usuário para solicitar um empréstimo.";
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetString(SessionAuth.UserIdKey);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var livro = await _context.Livros.FindAsync(id);
            if (livro == null)
                return NotFound();

            if (livro.Quantidade <= 0)
            {
                TempData["ErrorMessage"] = "Não há exemplares disponíveis para este título.";
                return RedirectToAction(nameof(Index));
            }

            var pendente = await _context.Emprestimos.AnyAsync(e =>
                e.IdUsuario == userId &&
                e.IdLivro == id &&
                e.AguardandoAprovacao &&
                e.DataDevolucaoReal == null);
            if (pendente)
            {
                TempData["ErrorMessage"] = "Você já possui uma solicitação pendente para este livro.";
                return RedirectToAction(nameof(Index));
            }

            var emprestimo = new Emprestimo
            {
                DataInicio = DateTime.Today,
                DataFim = DateTime.Today.AddDays(14),
                IdUsuario = userId,
                IdLivro = livro.Id,
                AguardandoAprovacao = true
            };
            _context.Emprestimos.Add(emprestimo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Solicitação enviada! Aguarde a aprovação no painel administrativo.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Livros/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString(SessionAuth.AuthRoleKey) != SessionAuth.RoleAdmin)
                return RedirectToAction(nameof(Index));

            ViewData["NavSection"] = "acervo";
            return View();
        }

        // POST: Livros/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titulo,Autor,Categoria,Quantidade")] Livro livro)
        {
            if (HttpContext.Session.GetString(SessionAuth.AuthRoleKey) != SessionAuth.RoleAdmin)
                return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                livro.Id = Guid.NewGuid().ToString();
                _context.Add(livro);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Livro cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(livro);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (HttpContext.Session.GetString(SessionAuth.AuthRoleKey) != SessionAuth.RoleAdmin)
                return RedirectToAction(nameof(Index));

            ViewData["NavSection"] = "acervo";
            if (id == null)
            {
                return NotFound();
            }

            var livro = await _context.Livros.FindAsync(id);
            if (livro == null)
            {
                return NotFound();
            }
            return View(livro);
        }

        // POST: Livros/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Titulo,Autor,Categoria,Quantidade")] Livro livro)
        {
            if (HttpContext.Session.GetString(SessionAuth.AuthRoleKey) != SessionAuth.RoleAdmin)
                return RedirectToAction(nameof(Index));

            if (id != livro.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(livro);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Livro atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LivroExists(livro.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(livro);
        }

        // GET: Livros/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (HttpContext.Session.GetString(SessionAuth.AuthRoleKey) != SessionAuth.RoleAdmin)
                return RedirectToAction(nameof(Index));

            ViewData["NavSection"] = "acervo";
            if (id == null)
            {
                return NotFound();
            }

            var livro = await _context.Livros
                .FirstOrDefaultAsync(m => m.Id == id);
            if (livro == null)
            {
                return NotFound();
            }

            return View(livro);
        }

        // POST: Livros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (HttpContext.Session.GetString(SessionAuth.AuthRoleKey) != SessionAuth.RoleAdmin)
                return RedirectToAction(nameof(Index));

            var livro = await _context.Livros.FindAsync(id);
            if (livro == null)
                return RedirectToAction(nameof(Index));

            var temEmprestimos = await _context.Emprestimos.AnyAsync(e => e.IdLivro == id);
            if (temEmprestimos)
            {
                TempData["ErrorMessage"] = "Não é possível excluir este livro pois existem empréstimos vinculados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Livros.Remove(livro);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Livro excluído com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        private bool LivroExists(string id)
        {
            return _context.Livros.Any(e => e.Id == id);
        }
    }
}
