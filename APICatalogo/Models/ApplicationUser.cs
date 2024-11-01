using Microsoft.AspNetCore.Identity;

namespace APICatalogo.Models;

public class ApplicationUser : IdentityUser //Representa o usuário do Identity
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}