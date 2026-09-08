using BibliotecaOnline.Models;

namespace BibliotecaOnline.ViewModels;

public class AdminPainelViewModel
{
    public int TotalLivros { get; set; }
    public int EmprestimosAtivos { get; set; }
    public int UsuariosRegistrados { get; set; }
    public int EmprestimosAtrasados { get; set; }
    public IList<Emprestimo> Pendentes { get; set; } = new List<Emprestimo>();
    public string AdminNome { get; set; } = "Admin";
}
