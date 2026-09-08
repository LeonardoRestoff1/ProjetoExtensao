using BibliotecaOnline.Models;

namespace BibliotecaOnline.ViewModels;

public class HomeIndexViewModel
{
    public int LivrosDisponiveis { get; set; }
    public int MeusEmprestimosAtivos { get; set; }
    public int ProximosVencimentos { get; set; }
    public IList<Livro> LivrosRecomendados { get; set; } = new List<Livro>();
    public string? SaudacaoNome { get; set; }
}
