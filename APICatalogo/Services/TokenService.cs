using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace APICatalogo.Services;

public class TokenService : ITokenService
{
    public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config)
    {
        //Obter a chave secreta definada no appsettings.json
        var key = _config.GetSection("JWT").GetValue<string>("SecretKey") ??
                  throw new NullReferenceException("SecretKey");
        //Converto a chave para um array de bytes
        var privateKey = Encoding.UTF8.GetBytes(key);

        //Criar as credencias de assinatura
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey),
            SecurityAlgorithms.HmacSha256Signature);

        //Informações que o token terá ao ser gerado
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),

            Expires = DateTime.UtcNow.AddMinutes(_config.GetSection("JWT")
                .GetValue<double>("TokenValidityInMinutes")),

            Audience = _config.GetSection("JWT").GetValue<string>("ValidAudience"),

            Issuer = _config.GetSection("JWT").GetValue<string>("ValidIssuer"), //Valor do emissor

            //atribuo as credencias criadas no passo acima
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

        return token;
    }

    public string GenerateRefreshToken()
    {
        //Crio um array de bytes vazia
        var secureRandomBytes = new byte[128];

        //Intancia de  RandomNumberGenerator
        using var randomNumberGenerator = RandomNumberGenerator.Create();

        //Populo a variavel secureRandomBytes com bytes aleatórios
        randomNumberGenerator.GetBytes(secureRandomBytes);

        //Converto o bytes para uma representação em string no formato base64
        var refreshToken = Convert.ToBase64String(secureRandomBytes);

        return refreshToken;
    }

    //Retorna as Clains do token expirado
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config)
    {
        var secretKey = _config.GetSection("JWT").GetValue<string>("SecretKey") ??
                        throw new NullReferenceException("SecretKey");

        //Parâmetros de validação do token
        var tokkenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false,
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        //Valida o token expirado com base nos parâmetros de validação configurados em tokkenValidationParameters
        //out SecurityToken securityToken -> Parâmetro de saída
        var principal = tokenHandler.ValidateToken(token, tokkenValidationParameters, out SecurityToken securityToken);

        /*
         * Se o parâmetro de saída securityToken não for uma instância de JwtSecurityToken
         * ou o algoritmo não for o HmacSha256, executamos uma exceção
         */
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase)
           )
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}