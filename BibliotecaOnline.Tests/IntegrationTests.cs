using System.Net;
using BibliotecaOnline.Data;
using BibliotecaOnline.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BibliotecaOnline.Tests;

public class IntegrationTests : IClassFixture<BibliotecaOnlineWebAppFactory>
{
    private readonly BibliotecaOnlineWebAppFactory _factory;

    public IntegrationTests(BibliotecaOnlineWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Home_RetornaPaginaInicialComSucesso()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Biblioteca Online", html);
    }

    [Fact]
    public async Task Acervo_ListaLivrosDoBanco()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Livros");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Dom Casmurro", html);
        Assert.Contains("Consulta do acervo", html);
    }

    [Fact]
    public async Task Acervo_FiltraPorCategoria()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Livros?categoria=Tecnologia");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Algoritmos", html);
        Assert.DoesNotContain("Dom Casmurro", html);
    }

    [Fact]
    public async Task Login_Admin_RedirecionaParaPainel()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await TestHelpers.LoginAsAdminAsync(client);
        var admin = await client.GetAsync("/Admin");
        admin.EnsureSuccessStatusCode();
        var html = await admin.Content.ReadAsStringAsync();
        Assert.Contains("Painel", html);
    }

    [Fact]
    public async Task Login_Leitor_RedirecionaParaHome()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await TestHelpers.LoginAsUserAsync(client);

        var home = await client.GetAsync("/");
        home.EnsureSuccessStatusCode();
        var html = await home.Content.ReadAsStringAsync();
        Assert.Contains("João Silva", html);
    }

    [Fact]
    public async Task Login_CredenciaisInvalidas_RetornaErro()
    {
        var client = _factory.CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = TestHelpers.ExtractAntiforgeryToken(html);

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Login"] = "inexistente@email.com",
            ["Senha"] = "senhaerrada",
            ["__RequestVerificationToken"] = token
        }));

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("E-mail ou senha inválidos", body);
    }

    [Fact]
    public async Task SolicitarEmprestimo_CriaRegistroPendente()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await TestHelpers.LoginAsUserAsync(client);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
        var livro = await db.Livros.FirstAsync(l => l.Titulo == "1984");
        var usuario = await db.Usuarios.FirstAsync(u => u.Email == "joao@email.com");

        var acervo = await client.GetAsync("/Livros");
        var html = await acervo.Content.ReadAsStringAsync();
        var token = TestHelpers.ExtractAntiforgeryToken(html);

        var response = await client.PostAsync("/Livros/Solicitar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = livro.Id,
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var pendente = await db.Emprestimos.AnyAsync(e =>
            e.IdUsuario == usuario.Id &&
            e.IdLivro == livro.Id &&
            e.AguardandoAprovacao);

        Assert.True(pendente);
    }

    [Fact]
    public async Task AprovarEmprestimo_DecrementaEstoque()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await TestHelpers.LoginAsAdminAsync(client);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
        var livro = await db.Livros.FirstAsync(l => l.Titulo == "Algoritmos");
        var usuario = await db.Usuarios.FirstAsync(u => u.Email == "joao@email.com");
        var qtdAntes = livro.Quantidade;

        var emprestimo = new Emprestimo
        {
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today.AddDays(14),
            IdUsuario = usuario.Id,
            IdLivro = livro.Id,
            AguardandoAprovacao = true
        };
        db.Emprestimos.Add(emprestimo);
        await db.SaveChangesAsync();

        var adminPage = await client.GetAsync("/Admin");
        var html = await adminPage.Content.ReadAsStringAsync();
        var token = TestHelpers.ExtractAntiforgeryToken(html);

        var response = await client.PostAsync("/Emprestimos/Aprovar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = emprestimo.Id.ToString(),
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        await db.Entry(livro).ReloadAsync();
        Assert.Equal(qtdAntes - 1, livro.Quantidade);
    }

    [Fact]
    public async Task ExcluirLivroComEmprestimo_RetornaMensagemAmigavel()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await TestHelpers.LoginAsAdminAsync(client);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
        var livro = await db.Livros.FirstAsync(l => l.Titulo == "Dom Casmurro");

        var deletePage = await client.GetAsync($"/Livros/Delete/{livro.Id}");
        var html = await deletePage.Content.ReadAsStringAsync();
        var token = TestHelpers.ExtractAntiforgeryToken(html);

        var response = await client.PostAsync($"/Livros/Delete/{livro.Id}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var acervo = await client.GetAsync("/Livros");
        var acervoHtml = await acervo.Content.ReadAsStringAsync();
        Assert.Contains("Não é possível excluir este livro", acervoHtml);
    }
}
