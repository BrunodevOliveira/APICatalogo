using System.Reflection;
using System.Security.Claims;
using System.Text;
using APICatalogo.Application.DTOs.Mappings;         // Updated
using APICatalogo.Application.Interfaces.Services;  // Updated (for ITokenService)
using APICatalogo.Domain.Entities;                // Updated (for ApplicationUser)
using APICatalogo.Domain.Interfaces;              // Updated (for Repository Interfaces)
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Infrastructure.Data;            // Updated (for AppDbContext)
using APICatalogo.Infrastructure.Data.Repositories; // Updated (for Repository Implementations)
using APICatalogo.Infrastructure.Logging;         // Updated
using APICatalogo.Infrastructure.RateLimiting;    // Updated
using APICatalogo.Infrastructure.Services;        // Updated (for TokenService implementation)
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var secretKey = builder.Configuration["JWT:SecretKey"] ?? 
            throw new ArgumentNullException("Invalid secret Key!");

// Add services to the container.

builder.Services
    .AddControllers(options => options.Filters.Add(typeof(APIExceptionFilter)))
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
    .AddNewtonsoftJson();               

var OrigensComAcessoPermitido = "_origensComAcessoPermitido";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: OrigensComAcessoPermitido,
        policy =>
        {
            policy
                .WithOrigins("https://apirequest.io", "https://www.outrosite.com")
                .WithMethods("GET", "POST")
                .AllowAnyHeader() // Permite qualquer cabeçalho
                .AllowCredentials();
                //.AllowAnyMethod()   // Permite todos os métodos HTTP
                //.AllowAnyHeader() // Permite o envio de credências
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "APICatalogo",
        Description = "Catálogos de produtos e Categorias",
        TermsOfService = new Uri("https://www.google.com"),
        Contact = new OpenApiContact
        {
            Name = "Bruno",
            Email = "brunodevoliveira@gmail.com", 
            Url = new Uri("https://www.linkedin.com/in/brunodevoliveira/")
        },
        License = new OpenApiLicense
        {
            Name = "Usar sobre LICX",
            Url = new Uri("https://www.linkedin.com/in/brunodevoliveira/")
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer JWT."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});



// builder.Services.AddAuthentication("Bearer").AddJwtBearer(); //Configuração básica de autenticação, substituída pela mais completa  abaixo

//Configuração do Indentity
builder.Services.AddIdentity<APICatalogo.Domain.Entities.ApplicationUser, IdentityRole>() 
    .AddEntityFrameworkStores<APICatalogo.Infrastructure.Data.AppDbContext>() 
    .AddDefaultTokenProviders(); 

//Habilitando e configurando autenticação JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;//Define Token JWT como Autenticação
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;//Challenge = Login para usuário sem token
}).AddJwtBearer(options =>
{
    //Configuração de validação do token
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;//Em produção é true
    options.TokenValidationParameters = new TokenValidationParameters() //Configura parâmetros de validação do token
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
    };
});

//Configuração da Autorização:
builder.Services.AddAuthorization(options =>
{   
    //RequireRole -> Exige que o usuário tenha uma determinada Role para acessar o recurso
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    
    //RequireClaim -> Exige que o usuário tenha uma Claim específica para acessar um recurso protegido 
    options.AddPolicy("SuperAdminOnly", policy => 
        policy.RequireRole("Admin").RequireClaim("id", "bruno"));
    
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    
    //RequireAssertion -> Permite definir uma expressão lambda com uma condição customizada para autorização
    options.AddPolicy("ExclusivePolicyOnly", policy => 
        policy.RequireAssertion(context => 
            context.User.HasClaim(claim => claim.Type == "id" && claim.Value == "bruno") 
                                           || context.User.IsInRole("SuperAdmin")));
});

//Aplica Limitação de taxa 
var myOptions = new APICatalogo.Infrastructure.RateLimiting.MyRateLimitOptions();
//Faz o Bind entre os valores padrões definidos no appsettings com os valores defaults definidos na classe MyRate:
builder.Configuration.GetSection(APICatalogo.Infrastructure.RateLimiting.MyRateLimitOptions.MyRateLimit).Bind(myOptions);

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixedwindow", options =>
    {
        options.PermitLimit = myOptions.PermitLimit;//1;
        options.Window = TimeSpan.FromSeconds(myOptions.Window);//TimeSpan.FromSeconds(5);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = myOptions.QueueLimit;
    });
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

//Configuração do Versionamento da API
builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
    o.ApiVersionReader = ApiVersionReader.Combine( //Configura os tipos de versionamento disponíveis na API
        new QueryStringApiVersionReader(),
        new UrlSegmentApiVersionReader()
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

string mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");

//Registro do EF no container DI nativo usando o m�todo AddDbContext
builder.Services.AddDbContext<APICatalogo.Infrastructure.Data.AppDbContext>(options => // AppDbContext from Infrastructure
    options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection)));

builder.Services.AddScoped<ApiLoggingFilter>();

// Register UnitOfWork. Specific repositories ICategoriaRepository and IProdutoRepository
// are exposed via IUnitOfWork properties and resolved by UnitOfWork's constructor.
// The generic IRepository<> is also not typically resolved directly when UoW is used.
builder.Services.AddScoped<APICatalogo.Domain.Interfaces.IUnitOfWork, APICatalogo.Infrastructure.Data.Repositories.UnitOfWork>();
// Removing individual repository registrations as they are accessible via IUnitOfWork
// builder.Services.AddScoped<APICatalogo.Domain.Interfaces.ICategoriaRepository, APICatalogo.Infrastructure.Data.Repositories.CategoriaRepository>();
// builder.Services.AddScoped<APICatalogo.Domain.Interfaces.IProdutoRepository, APICatalogo.Infrastructure.Data.Repositories.ProdutoRepository>();
// builder.Services.AddScoped(typeof(APICatalogo.Domain.Interfaces.IRepository<>), typeof(APICatalogo.Infrastructure.Data.Repositories.Repository<>));


builder.Services.AddAutoMapper(typeof(APICatalogo.Application.DTOs.Mappings.ProdutoDTOMappingProfile)); // ProdutoDTOMappingProfile from Application

//Registro no container DI do Service de Token
builder.Services.AddScoped<APICatalogo.Application.Interfaces.Services.ITokenService, APICatalogo.Infrastructure.Services.TokenService>();

// Adiciona o provedor de log personalizado (CustomLoggerProvider) ao sistema de log do ASP.NETCode, definindo o n�vel m�nimo de log como LogLevel.Information
builder.Logging.AddProvider(new APICatalogo.Infrastructure.Logging.CustomLoggerPrivider( // CustomLoggerPrivider from Infrastructure
    new APICatalogo.Infrastructure.Logging.CustomLoggerProviderConfiguration // CustomLoggerProviderConfiguration from Infrastructure
    {
        LogLevel = LogLevel.Information
    }));

builder.Services.AddControllers();

var app = builder.Build();

// Middlewares adicionados geralmente possuem a nomenclatura "Use" no in�cio:

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ConfigurationExceptionHandler();
}
app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseCors(OrigensComAcessoPermitido);

app.UseAuthorization();

//Middleware Personalizado:
//app.Use(async (context, next) =>
//{
//    //adicionar codigo antes do request
//    await next(context);
//    //adicionar o c�digo depois do request
//});

// Middleware terminal:

//app.Run(async (context) =>
//{
//    await context.Response.WriteAsync("Middleware final");
//});

app.MapControllers();

app.Run();
