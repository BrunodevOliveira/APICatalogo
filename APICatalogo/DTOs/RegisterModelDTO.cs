using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs;

public class RegisterModelDTO
{
    [Required(ErrorMessage = "Nome do usuário é obrigatório")]
    public string?  UserName { get; set; }
    
    [EmailAddress]
    [Required(ErrorMessage = "Email é obrigatório")]
    public string? Email { get; set; }
    
    [Required(ErrorMessage = "Senha é obrigatório")]
    public string?  Password { get; set; }
}