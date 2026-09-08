using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaOnline.Data;
using BibliotecaOnline.Infrastructure;
using BibliotecaOnline.Models;

namespace BibliotecaOnline.Controllers;

public class AdministradoresController : Controller
{
    private readonly BibliotecaDbContext _context;

    public AdministradoresController(BibliotecaDbContext context)
    {
        _context = context;
    }

    private bool IsAdmin() =>
        HttpContext.Session.GetString(SessionAuth.AuthRoleKey) == SessionAuth.RoleAdmin;

    public async Task<IActionResult> Index()
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Home");
        }

        ViewData["NavSection"] = "administradores";
        var lista = await _context.Administradores
            .AsNoTracking()
            .OrderBy(a => a.Nome)
            .ToListAsync();
        return View(lista);
    }

    public IActionResult Create()
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Home");
        }

        ViewData["NavSection"] = "administradores";
        return View(new Administrador());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Email,Senha")] Administrador admin)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Home");
        }

        ViewData["NavSection"] = "administradores";

        var emailNorm = (admin.Email ?? "").Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(emailNorm) &&
            await _context.Administradores.AnyAsync(a => a.Email.ToLower() == emailNorm))
        {
            ModelState.AddModelError(nameof(admin.Email), "Este e-mail já está cadastrado.");
        }

        if (string.IsNullOrWhiteSpace(admin.Senha) || admin.Senha.Trim().Length < 4)
        {
            ModelState.AddModelError(nameof(admin.Senha), "Informe uma senha com pelo menos 4 caracteres.");
        }

        if (!ModelState.IsValid)
            return View(admin);

        admin.Id = Guid.NewGuid().ToString();
        admin.Nome = admin.Nome.Trim();
        admin.Email = emailNorm;
        admin.Senha = admin.Senha.Trim();
        _context.Administradores.Add(admin);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Administrador cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Home");
        }

        if (string.IsNullOrEmpty(id))
            return NotFound();

        var total = await _context.Administradores.CountAsync();
        if (total <= 1)
        {
            TempData["ErrorMessage"] = "É obrigatório existir pelo menos um administrador no sistema.";
            return RedirectToAction(nameof(Index));
        }

        var atualId = HttpContext.Session.GetString(SessionAuth.AdminIdKey);
        if (id == atualId)
        {
            TempData["ErrorMessage"] = "Você não pode excluir o administrador com o qual está logado.";
            return RedirectToAction(nameof(Index));
        }

        var entity = await _context.Administradores.FindAsync(id);
        if (entity != null)
        {
            _context.Administradores.Remove(entity);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Administrador removido.";
        }

        return RedirectToAction(nameof(Index));
    }
}
