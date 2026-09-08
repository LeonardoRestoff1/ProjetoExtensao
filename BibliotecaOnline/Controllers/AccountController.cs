using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaOnline.Data;
using BibliotecaOnline.Infrastructure;
using BibliotecaOnline.Models;
using BibliotecaOnline.ViewModels;

namespace BibliotecaOnline.Controllers;

public class AccountController : Controller
{
    private readonly BibliotecaDbContext _context;

    public AccountController(BibliotecaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        ViewData["NavSection"] = "login";
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["NavSection"] = "login";
        if (!ModelState.IsValid)
            return View(model);

        var login = model.Login.Trim();
        var senha = model.Senha?.Trim() ?? string.Empty;
        var loginLower = login.ToLowerInvariant();

        var admin = await _context.Administradores
            .FirstOrDefaultAsync(a =>
                a.Email.ToLower() == loginLower && a.Senha == senha);
        if (admin != null)
        {
            HttpContext.Session.SetString(SessionAuth.AuthRoleKey, SessionAuth.RoleAdmin);
            HttpContext.Session.SetString(SessionAuth.AdminIdKey, admin.Id);
            HttpContext.Session.SetString(SessionAuth.UserNameKey, admin.Nome);
            HttpContext.Session.Remove(SessionAuth.UserIdKey);
            TempData["SuccessMessage"] = $"Bem-vindo(a), {admin.Nome}!";
            return RedirectToAction("Index", "Admin");
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == loginLower);

        if (usuario != null && usuario.Senha.Trim() == senha)
        {
            HttpContext.Session.SetString(SessionAuth.AuthRoleKey, SessionAuth.RoleUser);
            HttpContext.Session.SetString(SessionAuth.UserIdKey, usuario.Id);
            HttpContext.Session.SetString(SessionAuth.UserNameKey, usuario.Nome);
            HttpContext.Session.Remove(SessionAuth.AdminIdKey);
            TempData["SuccessMessage"] = $"Olá, {usuario.Nome}!";
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewData["NavSection"] = "login";
        return View(new Usuario());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([Bind("Nome,Email,Telefone,Senha")] Usuario usuario)
    {
        ViewData["NavSection"] = "login";

        var emailNorm = (usuario.Email ?? "").Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(emailNorm) &&
            await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == emailNorm))
        {
            ModelState.AddModelError(nameof(usuario.Email), "Este e-mail já está cadastrado.");
        }

        if (string.IsNullOrWhiteSpace(usuario.Senha) || usuario.Senha.Trim().Length < 4)
        {
            ModelState.AddModelError(nameof(usuario.Senha), "Informe uma senha com pelo menos 4 caracteres.");
        }

        if (!ModelState.IsValid)
            return View(usuario);

        usuario.Nome = usuario.Nome.Trim();
        usuario.Email = emailNorm;
        if (!string.IsNullOrEmpty(usuario.Telefone))
            usuario.Telefone = usuario.Telefone.Trim();
        usuario.Senha = usuario.Senha.Trim();

        usuario.Id = Guid.NewGuid().ToString();
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        HttpContext.Session.SetString(SessionAuth.AuthRoleKey, SessionAuth.RoleUser);
        HttpContext.Session.SetString(SessionAuth.UserIdKey, usuario.Id);
        HttpContext.Session.SetString(SessionAuth.UserNameKey, usuario.Nome);
        TempData["SuccessMessage"] = "Cadastro realizado! Você já está logado.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["SuccessMessage"] = "Você saiu do sistema.";
        return RedirectToAction("Index", "Home");
    }
}
