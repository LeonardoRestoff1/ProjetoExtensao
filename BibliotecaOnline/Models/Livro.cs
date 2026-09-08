using System.ComponentModel.DataAnnotations;

namespace BibliotecaOnline.Models
{
    public class Livro
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "O título é obrigatório")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O autor é obrigatório")]
        [Display(Name = "Autor")]
        public string Autor { get; set; } = string.Empty;
        
        [Display(Name = "Categoria")]
        public string? Categoria { get; set; }
        
        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        [Display(Name = "Quantidade Disponível")]
        public int Quantidade { get; set; }
        
        public ICollection<Emprestimo>? Emprestimos { get; set; }
    }
}
