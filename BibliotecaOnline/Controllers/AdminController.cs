using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaOnline.Data;
using BibliotecaOnline.Infrastructure;
using BibliotecaOnline.ViewModels;

namespace BibliotecaOnline.Controllers;

public class AdminController : Controller
{
    private readonly BibliotecaDbContext _context;

    public AdminController(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString(SessionAuth.AuthRoleKey) != SessionAuth.RoleAdmin)
            return RedirectToAction("Login", "Account");

        ViewData["NavSection"] = "admin";

        var totalLivros = await _context.Livros.SumAsync(l => (int?)l.Quantidade) ?? 0;
        var ativos = await _context.Emprestimos.CountAsync(e =>
            e.DataDevolucaoReal == null && !e.AguardandoAprovacao);
        var usuarios = await _context.Usuarios.CountAsync();
        var atrasados = await _context.Emprestimos.CountAsync(e =>
            e.DataDevolucaoReal == null && !e.AguardandoAprovacao && e.DataFim < DateTime.Today);

        var pendentes = await _context.Emprestimos
            .Include(e => e.Usuario)
            .Include(e => e.Livro)
            .Where(e => e.AguardandoAprovacao && e.DataDevolucaoReal == null)
            .OrderBy(e => e.DataInicio)
            .ToListAsync();

        var adminNome = HttpContext.Session.GetString(SessionAuth.UserNameKey) ?? "Admin";

        var vm = new AdminPainelViewModel
        {
            TotalLivros = totalLivros,
            EmprestimosAtivos = ativos,
            UsuariosRegistrados = usuarios,
            EmprestimosAtrasados = atrasados,
            Pendentes = pendentes,
            AdminNome = adminNome
        };

        return View(vm);
    }
}
