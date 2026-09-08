using BibliotecaOnline.Models;

namespace BibliotecaOnline.Data;

public static class DbInitializer
{
    public static void Seed(BibliotecaDbContext context)
    {
        if (context.Administradores.Any())
            return;

        var admin = new Administrador
        {
            Id = Guid.NewGuid().ToString(),
            Nome = "Maria Santos",
            Email = "admin@biblioteca.com",
            Senha = "admin123"
        };
        context.Administradores.Add(admin);

        var u1 = new Usuario
        {
            Id = Guid.NewGuid().ToString(),
            Nome = "João Silva",
            Email = "joao@email.com",
            Telefone = "(11) 98888-7777",
            Senha = "123456"
        };
        var u2 = new Usuario
        {
            Id = Guid.NewGuid().ToString(),
            Nome = "Ana Costa",
            Email = "ana@email.com",
            Telefone = "(21) 97777-6666",
            Senha = "123456"
        };
        context.Usuarios.AddRange(u1, u2);

        var l1 = new Livro
        {
            Id = Guid.NewGuid().ToString(),
            Titulo = "Dom Casmurro",
            Autor = "Machado de Assis",
            Categoria = "Literatura",
            Quantidade = 5
        };
        var l2 = new Livro
        {
            Id = Guid.NewGuid().ToString(),
            Titulo = "Algoritmos",
            Autor = "Thomas Cormen",
            Categoria = "Tecnologia",
            Quantidade = 3
        };
        var l3 = new Livro
        {
            Id = Guid.NewGuid().ToString(),
            Titulo = "História do Brasil",
            Autor = "Boris Fausto",
            Categoria = "História",
            Quantidade = 0
        };
        var l4 = new Livro
        {
            Id = Guid.NewGuid().ToString(),
            Titulo = "O Alquimista",
            Autor = "Paulo Coelho",
            Categoria = "Literatura",
            Quantidade = 4
        };
        var l5 = new Livro
        {
            Id = Guid.NewGuid().ToString(),
            Titulo = "1984",
            Autor = "George Orwell",
            Categoria = "Literatura",
            Quantidade = 2
        };
        context.Livros.AddRange(l1, l2, l3, l4, l5);

        context.SaveChanges();

        // Empréstimo ativo (protótipo)
        context.Emprestimos.Add(new Emprestimo
        {
            DataInicio = DateTime.Today.AddDays(-20),
            DataFim = DateTime.Today.AddDays(-6),
            IdUsuario = u1.Id,
            IdLivro = l1.Id,
            AguardandoAprovacao = false
        });
        // Consome 1 exemplar
        l1.Quantidade--;

        // Empréstimo atrasado
        context.Emprestimos.Add(new Emprestimo
        {
            DataInicio = DateTime.Today.AddDays(-30),
            DataFim = DateTime.Today.AddDays(-16),
            IdUsuario = u1.Id,
            IdLivro = l4.Id,
            AguardandoAprovacao = false
        });
        l4.Quantidade--;

        // Pendentes de aprovação (protótipo painel admin)
        context.Emprestimos.Add(new Emprestimo
        {
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today.AddDays(14),
            IdUsuario = u1.Id,
            IdLivro = l2.Id,
            AguardandoAprovacao = true
        });
        context.Emprestimos.Add(new Emprestimo
        {
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today.AddDays(14),
            IdUsuario = u2.Id,
            IdLivro = l1.Id,
            AguardandoAprovacao = true
        });

        // Histórico devolvido
        context.Emprestimos.Add(new Emprestimo
        {
            DataInicio = DateTime.Today.AddMonths(-2),
            DataFim = DateTime.Today.AddMonths(-2).AddDays(14),
            DataDevolucaoReal = DateTime.Today.AddMonths(-2).AddDays(12),
            IdUsuario = u1.Id,
            IdLivro = l5.Id,
            AguardandoAprovacao = false
        });

        context.SaveChanges();
    }
}
