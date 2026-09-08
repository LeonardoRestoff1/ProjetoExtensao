using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaOnline.Data;
using BibliotecaOnline.Infrastructure;
using BibliotecaOnline.Models;
using BibliotecaOnline.ViewModels;

namespace BibliotecaOnline.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BibliotecaDbContext _context;

    public HomeController(ILogger<HomeController> logger, BibliotecaDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["NavSection"] = "home";

        var livrosDisponiveis = await _context.Livros.SumAsync(l => (int?)l.Quantidade) ?? 0;
        var recomendados = await _context.Livros.AsNoTracking().Take(3).ToListAsync();

        var vm = new HomeIndexViewModel
        {
            LivrosDisponiveis = livrosDisponiveis,
            LivrosRecomendados = recomendados,
            SaudacaoNome = HttpContext.Session.GetString(SessionAuth.UserNameKey)
        };

        var role = HttpContext.Session.GetString(SessionAuth.AuthRoleKey);
        var userId = HttpContext.Session.GetString(SessionAuth.UserIdKey);

        if (role == SessionAuth.RoleUser && !string.IsNullOrEmpty(userId))
        {
            vm.MeusEmprestimosAtivos = await _context.Emprestimos.CountAsync(e =>
                e.IdUsuario == userId &&
                e.DataDevolucaoReal == null);

            var limite = DateTime.Today.AddDays(3);
            vm.ProximosVencimentos = await _context.Emprestimos.CountAsync(e =>
                e.IdUsuario == userId &&
                e.DataDevolucaoReal == null &&
                !e.AguardandoAprovacao &&
                e.DataFim >= DateTime.Today &&
                e.DataFim <= limite);
        }

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
