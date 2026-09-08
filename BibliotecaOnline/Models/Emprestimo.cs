using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaOnline.Models
{
    public class Emprestimo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória")]
        [Display(Name = "Data de Início")]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "A data de previsão de devolução é obrigatória")]
        [Display(Name = "Previsão de Devolução")]
        [DataType(DataType.Date)]
        public DateTime DataFim { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Data de Devolução Real")]
        [DataType(DataType.Date)]
        public DateTime? DataDevolucaoReal { get; set; }

        [Required(ErrorMessage = "O usuário é obrigatório")]
        [ForeignKey("Usuario")]
        [Display(Name = "Usuário")]
        public string IdUsuario { get; set; } = string.Empty;

        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "O livro é obrigatório")]
        [ForeignKey("Livro")]
        [Display(Name = "Livro")]
        public string IdLivro { get; set; } = string.Empty;

        public Livro? Livro { get; set; }

        [Display(Name = "Aguardando aprovação")]
        public bool AguardandoAprovacao { get; set; }
    }
}
