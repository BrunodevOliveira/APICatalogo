using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthController(ITokenService tokenService, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    //Gera o REfreshToken e Token para a sessão
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModelDTO model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName!); //Verifica se o usuário existe
        var senhaUserValida = await _userManager.CheckPasswordAsync(user, model.Password!);

        if (user is not null && senhaUserValida)
        {
            var userRoles = await _userManager.GetRolesAsync(user); //Perfis do usuário logado
            
            //Informações do usuário que serão incluidas no token
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),//Gera um Id para o token
            };
            
            //Inclui os perfis na lista de claims do usuário
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            
            var token  = _tokenService.GenerateAccessToken(authClaims, _configuration);//Gera o token
            
            
            //Converte o valor atribuido ao REfreshToken em appsettings.json para inteiro e atribui a variável refreshTokenValidityInMinutes
            // _ -> é uma variável que usamos quando não estamos interessados no retorno da operação
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);
            
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);
            
            user.RefreshToken = refreshToken;
            
            await _userManager.UpdateAsync(user); //Armazena o RefreshToken e RefreshTokenExpiryTime na tabela ASPNetUser do Identity
            
            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = token.ValidTo
            }); 
        }
        
        return Unauthorized();
    }

    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModelDTO model)
    {
        var userExist = await _userManager.FindByNameAsync(model.UserName!);

        if (userExist is not null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ResponseDTO() {Status = "Error", Message = "User already exists!"});
        }

        ApplicationUser user = new()
        {
            UserName = model.UserName,
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        
        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ResponseDTO() {Status = "Error", Message = "User creation failed!"});
        }
        
        return Ok(new ResponseDTO() {Status = "Success", Message = "User created successfully!"}); 
    }

    // Retorna um novo token JWT a partir do Token expiradoe e do  RefreshToken
    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshToken(TokenModelDTO tokenModel) //Recebe o RefreshToken e o token JWT expirado
    {
        if (tokenModel is null)
        {
            return BadRequest("Invalid client request");
        }
        
        //token JWT expirado
        string? accessToken = tokenModel.AcessToken ?? throw new ArgumentNullException(nameof(tokenModel.AcessToken));
        
        string? refreshToken = tokenModel.RefrehToken ?? throw new ArgumentNullException(nameof(tokenModel.RefrehToken));
        
        //Extrai as Claims do token JWT expirado para gerar um novo token
        var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration);

        if (principal == null)
        {
            return BadRequest("Invalid access token/refresh token");
        }
        
        string username = principal.Identity.Name;
        
        //Busca o usuário no banco
        var user = await _userManager.FindByNameAsync(username!);
        
        /*
         * Verifica se o usuário existe;
         * Compara o refreshToken armazenado na tabela para esse usuário com o  informado no request;
         * Compara a data do refreshToken com a data atual
         */
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        {
            return BadRequest("Invalid access token/refresh token");
        }
        
        //Caso o refreshToken for valid, gero um novo token de acesso
        var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _configuration);
        
        //Crio um novo refreshToken
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        
        //Atualizo o valor do RefreshToken do usuário na tabela
        user.RefreshToken = newRefreshToken;
        
        await _userManager.UpdateAsync(user);

        //Retorna um novo Token de acesso e um novo refreshToken que foi armazenado na tabela para o usuário 
        return new ObjectResult(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            refreshToken = newRefreshToken,
        });
    }

    [Authorize] //Somente um usuário autenticado poderá acessar essa rota
    [HttpPost]
    [Route("revoke/{username}")]
    public async Task<IActionResult> Revoke(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        
        if(user is null) return BadRequest("Invalid user name");
        
        user.RefreshToken = null;
        
        await _userManager.UpdateAsync(user);
        return NoContent();
    }
}