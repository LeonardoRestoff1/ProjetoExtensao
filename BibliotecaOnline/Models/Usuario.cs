using System.ComponentModel.DataAnnotations;

namespace BibliotecaOnline.Models
{
    public class Usuario
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [DataType(DataType.Password)]
        [StringLength(200)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;
        
        public ICollection<Emprestimo>? Emprestimos { get; set; }
    }
}
