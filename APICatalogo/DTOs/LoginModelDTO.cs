using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs;

public class LoginModelDTO
{
    [Required(ErrorMessage = "Nome do usuário é obrigatório")]
    public string?  UserName { get; set; }
    
    [Required(ErrorMessage = "Senha é obrigatório")]
    public string?  Password { get; set; }
    
    
}