using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BibliotecaOnline.Data;
using BibliotecaOnline.Infrastructure;
using BibliotecaOnline.Models;

namespace BibliotecaOnline.Controllers
{
    public class EmprestimosController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public EmprestimosController(BibliotecaDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() =>
            HttpContext.Session.GetString(SessionAuth.AuthRoleKey) == SessionAuth.RoleAdmin;

        private bool IsUser() =>
            HttpContext.Session.GetString(SessionAuth.AuthRoleKey) == SessionAuth.RoleUser;

        private string? CurrentUserId() =>
            HttpContext.Session.GetString(SessionAuth.UserIdKey);

        private bool PodeGerenciarEmprestimo(Emprestimo e)
        {
            if (IsAdmin()) return true;
            return IsUser() && CurrentUserId() == e.IdUsuario;
        }

        // GET: Empréstimos (operacional — admin)
        public async Task<IActionResult> Index(string? filtro)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Acesso restrito ao painel administrativo.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["NavSection"] = "emprestimos";

            var emprestimos = _context.Emprestimos
                .Include(e => e.Usuario)
                .Include(e => e.Livro)
                .Where(e => !e.AguardandoAprovacao)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
            {
                emprestimos = emprestimos.Where(e =>
                    (e.Usuario != null && e.Usuario.Nome.Contains(filtro)) ||
                    (e.Livro != null && e.Livro.Titulo.Contains(filtro)));
            }

            ViewBag.Filtro = filtro;
            return View(await emprestimos.OrderByDescending(e => e.DataInicio).ToListAsync());
        }

        // GET: Meus Empréstimos (usuário logado)
        public async Task<IActionResult> MeusEmprestimos()
        {
            if (!IsUser())
            {
                TempData["ErrorMessage"] = "Faça login para ver seus empréstimos.";
                return RedirectToAction("Login", "Account");
            }

            ViewData["NavSection"] = "meus";

            var userId = CurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var todos = await _context.Emprestimos
                .Include(e => e.Livro)
                .Where(e => e.IdUsuario == userId)
                .OrderByDescending(e => e.DataInicio)
                .ToListAsync();

            var ativos = todos.Where(e => e.DataDevolucaoReal == null).ToList();
            var historico = todos.Where(e => e.DataDevolucaoReal != null).ToList();

            ViewBag.Ativos = ativos;
            ViewBag.Historico = historico;
            return View();
        }

        // GET: Emprestimos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Usuario)
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (emprestimo == null)
                return NotFound();

            if (!PodeGerenciarEmprestimo(emprestimo))
            {
                TempData["ErrorMessage"] = "Você não tem permissão para ver este empréstimo.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["NavSection"] = IsAdmin() ? "emprestimos" : "meus";
            return View(emprestimo);
        }

        // GET: Emprestimos/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            ViewData["NavSection"] = "emprestimos";
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "Id", "Nome");
            ViewData["IdLivro"] = new SelectList(_context.Livros.Where(l => l.Quantidade > 0), "Id", "Titulo");
            return View();
        }

        // POST: Emprestimos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DataInicio,DataFim,IdUsuario,IdLivro")] Emprestimo emprestimo)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            emprestimo.AguardandoAprovacao = false;

            if (ModelState.IsValid)
            {
                var livro = await _context.Livros.FindAsync(emprestimo.IdLivro);
                if (livro == null)
                {
                    ModelState.AddModelError("IdLivro", "Livro não encontrado.");
                    ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "Id", "Nome", emprestimo.IdUsuario);
                    ViewData["IdLivro"] = new SelectList(_context.Livros.Where(l => l.Quantidade > 0), "Id", "Titulo", emprestimo.IdLivro);
                    return View(emprestimo);
                }

                if (livro.Quantidade <= 0)
                {
                    ModelState.AddModelError("IdLivro", "Não há exemplares disponíveis deste livro.");
                    ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "Id", "Nome", emprestimo.IdUsuario);
                    ViewData["IdLivro"] = new SelectList(_context.Livros.Where(l => l.Quantidade > 0), "Id", "Titulo", emprestimo.IdLivro);
                    return View(emprestimo);
                }

                livro.Quantidade--;
                _context.Update(livro);

                _context.Add(emprestimo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Empréstimo registrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "Id", "Nome", emprestimo.IdUsuario);
            ViewData["IdLivro"] = new SelectList(_context.Livros.Where(l => l.Quantidade > 0), "Id", "Titulo", emprestimo.IdLivro);
            return View(emprestimo);
        }

        // GET: Emprestimos/Devolver/5
        public async Task<IActionResult> Devolver(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Usuario)
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (emprestimo == null)
                return NotFound();

            if (!PodeGerenciarEmprestimo(emprestimo))
            {
                TempData["ErrorMessage"] = "Você não tem permissão para esta operação.";
                return RedirectToAction("Index", "Home");
            }

            if (emprestimo.AguardandoAprovacao)
            {
                TempData["ErrorMessage"] = "Este empréstimo ainda aguarda aprovação.";
                return RedirectToAction(IsAdmin() ? "Index" : nameof(MeusEmprestimos), IsAdmin() ? "Admin" : "Emprestimos");
            }

            if (emprestimo.DataDevolucaoReal != null)
            {
                TempData["ErrorMessage"] = "Este empréstimo já foi devolvido.";
                return RedirectToAction(IsUser() ? nameof(MeusEmprestimos) : nameof(Index));
            }

            ViewData["NavSection"] = IsUser() ? "meus" : "emprestimos";
            return View(emprestimo);
        }

        // POST: Emprestimos/Devolver/5
        [HttpPost, ActionName("Devolver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DevolverConfirmed(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (emprestimo == null)
                return NotFound();

            if (!PodeGerenciarEmprestimo(emprestimo))
            {
                TempData["ErrorMessage"] = "Você não tem permissão para esta operação.";
                return RedirectToAction("Index", "Home");
            }

            if (emprestimo.DataDevolucaoReal != null)
            {
                TempData["ErrorMessage"] = "Este empréstimo já foi devolvido.";
                return RedirectToAction(IsUser() ? nameof(MeusEmprestimos) : nameof(Index));
            }

            emprestimo.DataDevolucaoReal = DateTime.Now;

            var livro = emprestimo.Livro;
            if (livro != null && !emprestimo.AguardandoAprovacao)
            {
                livro.Quantidade++;
                _context.Update(livro);
            }

            _context.Update(emprestimo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Devolução registrada com sucesso!";
            return RedirectToAction(IsUser() ? nameof(MeusEmprestimos) : nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renovar(int id)
        {
            var emprestimo = await _context.Emprestimos.FirstOrDefaultAsync(e => e.Id == id);
            if (emprestimo == null)
                return NotFound();

            if (!PodeGerenciarEmprestimo(emprestimo))
            {
                TempData["ErrorMessage"] = "Você não tem permissão para esta operação.";
                return RedirectToAction("Index", "Home");
            }

            if (emprestimo.AguardandoAprovacao || emprestimo.DataDevolucaoReal != null)
            {
                TempData["ErrorMessage"] = "Não é possível renovar este empréstimo.";
                return RedirectToAction(nameof(MeusEmprestimos));
            }

            if (emprestimo.DataFim < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Empréstimos em atraso devem ser devolvidos antes de renovar.";
                return RedirectToAction(nameof(MeusEmprestimos));
            }

            emprestimo.DataFim = emprestimo.DataFim.AddDays(7);
            _context.Update(emprestimo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Prazo renovado por mais 7 dias.";
            return RedirectToAction(nameof(MeusEmprestimos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprovar(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null)
                return NotFound();

            if (!emprestimo.AguardandoAprovacao)
            {
                TempData["ErrorMessage"] = "Este empréstimo não está pendente.";
                return RedirectToAction("Index", "Admin");
            }

            var livro = emprestimo.Livro;
            if (livro == null || livro.Quantidade <= 0)
            {
                TempData["ErrorMessage"] = "Não há exemplares disponíveis para aprovar este empréstimo.";
                return RedirectToAction("Index", "Admin");
            }

            livro.Quantidade--;
            _context.Update(livro);
            emprestimo.AguardandoAprovacao = false;
            _context.Update(emprestimo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Empréstimo aprovado.";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rejeitar(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var emprestimo = await _context.Emprestimos.FindAsync(id);
            if (emprestimo == null)
                return NotFound();

            if (!emprestimo.AguardandoAprovacao)
            {
                TempData["ErrorMessage"] = "Este empréstimo não está pendente.";
                return RedirectToAction("Index", "Admin");
            }

            _context.Emprestimos.Remove(emprestimo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Solicitação rejeitada.";
            return RedirectToAction("Index", "Admin");
        }

        // GET: Emprestimos/Historico
        public async Task<IActionResult> Historico()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Acesso restrito ao painel administrativo.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["NavSection"] = "emprestimos";

            var emprestimos = await _context.Emprestimos
                .Include(e => e.Usuario)
                .Include(e => e.Livro)
                .OrderByDescending(e => e.DataInicio)
                .ToListAsync();

            return View(emprestimos);
        }
    }
}
