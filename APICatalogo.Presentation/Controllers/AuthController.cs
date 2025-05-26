using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APICatalogo.Application.DTOs;             // Updated
using APICatalogo.Application.Interfaces.Services; // Updated
using APICatalogo.Domain.Entities;            // Updated
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService; // Interface from Application
    private readonly UserManager<ApplicationUser> _userManager; // ApplicationUser from Domain
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokenService, UserManager<ApplicationUser> userManager, // ApplicationUser from Domain
        RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }
    
    // A role criada será armazenada na tabela aspnetroles criada pelo proprio Identity
    [HttpPost]
    [Route("CreateRole")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        var roleExist = await _roleManager.RoleExistsAsync(roleName);

        if (!roleExist)
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (roleResult.Succeeded)
            {
                _logger.LogInformation(1, "Role created successfully");
                return StatusCode(StatusCodes.Status200OK, 
                    new ResponseDTO() {Status = "Success", Message = $"Role {roleName} created successfully!"}); // ResponseDTO from Application
            }
            else
            {
                _logger.LogInformation(2, "Error");
                return StatusCode(StatusCodes.Status400BadRequest,
                    new ResponseDTO() {Status = "Error", Message = $"Issue adding the new {roleName} role"}); // ResponseDTO from Application
            }
        }
        
        return StatusCode(StatusCodes.Status400BadRequest,
            new ResponseDTO() {Status = "Error", Message = $"Role {roleName} already exists!"}); // ResponseDTO from Application
    }

    [HttpPost]
    [Route("AddUserToRole")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> AddUserToRole(string email, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(email); // ApplicationUser from Domain

        if (user != null)
        {
            var result = await _userManager.AddToRoleAsync(user, roleName); // ApplicationUser from Domain

            if (result.Succeeded)
            {
                _logger.LogInformation(1, $"User {user.Email} added to role {roleName}");
                return StatusCode(StatusCodes.Status200OK, 
                    new ResponseDTO() {Status = "Success", Message = $"User {user.Email} added to role {roleName}"}); // ResponseDTO from Application
            }
            else
            {
                _logger.LogInformation(2, $"Error: Unable to add user {user.Email} to role {roleName}");
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseDTO() {Status = "Error", Message =  // ResponseDTO from Application
                    $"Error: Unable to add user {user.Email} to role {roleName}"});
            }
        }
        
        return BadRequest(new { error = "Unable to find user" });
    }

    //Gera o RefreshToken e Token para a sessão
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModelDTO model) // LoginModelDTO from Application
    {
        var user = await _userManager.FindByNameAsync(model.UserName!); // ApplicationUser from Domain
        
        if (user is null) return Unauthorized("User not found."); // Check user before password

        var senhaUserValida = await _userManager.CheckPasswordAsync(user, model.Password!);

        if (senhaUserValida) // user is not null here
        {
            var userRoles = await _userManager.GetRolesAsync(user); // ApplicationUser from Domain
            
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("id", user.UserName!), // Consider using user.Id
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            
            var token  = _tokenService.GenerateAccessToken(authClaims, _configuration);
            
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);
            
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes); // ApplicationUser from Domain
            user.RefreshToken = refreshToken; // ApplicationUser from Domain
            
            await _userManager.UpdateAsync(user); // ApplicationUser from Domain
            
            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = token.ValidTo
            }); 
        }
        
        return Unauthorized("Invalid username or password.");
    }

    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModelDTO model) // RegisterModelDTO from Application
    {
        var userExist = await _userManager.FindByNameAsync(model.UserName!); // ApplicationUser from Domain

        if (userExist is not null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ResponseDTO() {Status = "Error", Message = "User already exists!"}); // ResponseDTO from Application
        }

        ApplicationUser user = new() // ApplicationUser from Domain
        {
            UserName = model.UserName,
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        
        var result = await _userManager.CreateAsync(user, model.Password!); // ApplicationUser from Domain

        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ResponseDTO() {Status = "Error", Message = "User creation failed!"}); // ResponseDTO from Application
        }
        
        return Ok(new ResponseDTO() {Status = "Success", Message = "User created successfully!"});  // ResponseDTO from Application
    }

    // Retorna um novo token JWT a partir do Token expiradoe e do  RefreshToken
    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshToken(TokenModelDTO tokenModel) // TokenModelDTO from Application
    {
        if (tokenModel is null)
        {
            return BadRequest("Invalid client request");
        }
        
        string? accessToken = tokenModel.AcessToken ?? throw new ArgumentNullException(nameof(tokenModel.AcessToken));
        string? refreshToken = tokenModel.RefrehToken ?? throw new ArgumentNullException(nameof(tokenModel.RefrehToken));
        
        var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration);

        if (principal?.Identity?.Name is null) // Check for null principal or Name
        {
            return BadRequest("Invalid access token/refresh token (principal error)");
        }
        
        string username = principal.Identity.Name;
        
        var user = await _userManager.FindByNameAsync(username!); // ApplicationUser from Domain
        
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        {
            return BadRequest("Invalid access token/refresh token (user/token mismatch or expiry)");
        }
        
        var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _configuration);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        
        user.RefreshToken = newRefreshToken; // ApplicationUser from Domain
        await _userManager.UpdateAsync(user); // ApplicationUser from Domain

        return new ObjectResult(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            refreshToken = newRefreshToken,
        });
    }

    [HttpPost]
    [Route("revoke/{username}")]
    [Authorize(Policy = "ExclusiveOnly")] 
    public async Task<IActionResult> Revoke(string username)
    {
        var user = await _userManager.FindByNameAsync(username); // ApplicationUser from Domain
        
        if(user is null) return BadRequest("Invalid user name");
        
        user.RefreshToken = null; // ApplicationUser from Domain
        
        await _userManager.UpdateAsync(user); // ApplicationUser from Domain
        return NoContent();
    }
}