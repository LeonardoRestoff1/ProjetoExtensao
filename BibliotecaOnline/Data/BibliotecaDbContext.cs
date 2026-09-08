using Microsoft.EntityFrameworkCore;
using BibliotecaOnline.Models;

namespace BibliotecaOnline.Data
{
    public class BibliotecaDbContext : DbContext
    {
        public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Livro> Livros { get; set; }
        public DbSet<Emprestimo> Emprestimos { get; set; }
        public DbSet<Administrador> Administradores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Telefone).HasMaxLength(20);
                entity.Property(e => e.Senha).IsRequired().HasMaxLength(200);
            });

            // Configuração de Livro
            modelBuilder.Entity<Livro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Autor).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Categoria).HasMaxLength(100);
                entity.Property(e => e.Quantidade).IsRequired();
            });

            // Configuração de Emprestimo
            modelBuilder.Entity<Emprestimo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataInicio).IsRequired();
                entity.Property(e => e.DataFim).IsRequired();
                entity.Property(e => e.IdUsuario).IsRequired();
                entity.Property(e => e.IdLivro).IsRequired();
                entity.Property(e => e.AguardandoAprovacao).IsRequired();

                // Relacionamento com Usuario
                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.Emprestimos)
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Livro
                entity.HasOne(e => e.Livro)
                    .WithMany(l => l.Emprestimos)
                    .HasForeignKey(e => e.IdLivro)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuração de Administrador
            modelBuilder.Entity<Administrador>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Senha).IsRequired().HasMaxLength(200);
            });
        }
    }
}






