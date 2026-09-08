using System.Net;
using System.Text.RegularExpressions;

namespace BibliotecaOnline.Tests;

internal static class TestHelpers
{
    internal static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            """name="__RequestVerificationToken" type="hidden" value="([^"]+)"""",
            RegexOptions.CultureInvariant);

        if (!match.Success)
            throw new InvalidOperationException("Token anti-forgery não encontrado na página.");

        return match.Groups[1].Value;
    }

    internal static async Task<HttpClient> LoginAsAdminAsync(HttpClient client)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Login"] = "admin@biblioteca.com",
            ["Senha"] = "admin123",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return client;
    }

    internal static async Task<HttpClient> LoginAsUserAsync(HttpClient client)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Login"] = "joao@email.com",
            ["Senha"] = "123456",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return client;
    }
}
