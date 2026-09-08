using Microsoft.EntityFrameworkCore;
using BibliotecaOnline.Data;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Sessão ───────────────────────────────────────────────────
var sessionHours = builder.Configuration.GetValue<int>("Session:IdleTimeoutHours", 4);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(sessionHours);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
});

// ── SQLite — caminho sempre na pasta do projeto (ContentRoot) ─
//    Em produção pode ser sobrescrito pela variável de ambiente
//    BIBLIOTECA_DB_PATH (ex.: /data/BibliotecaOnline.db em Docker)
var dbPathFromEnv = Environment.GetEnvironmentVariable("BIBLIOTECA_DB_PATH");
var dbPath = !string.IsNullOrEmpty(dbPathFromEnv)
    ? dbPathFromEnv
    : Path.Combine(builder.Environment.ContentRootPath, "BibliotecaOnline.db");

builder.Services.AddDbContext<BibliotecaDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// ── Porta configurável por variável de ambiente (Railway, Render, etc.) ──
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// ── Pipeline HTTP ─────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Migração e seed na inicialização ─────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BibliotecaDbContext>();
        context.Database.Migrate();
        DbInitializer.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao migrar ou popular o banco de dados.");
    }
}

app.Run();

public partial class Program { }
