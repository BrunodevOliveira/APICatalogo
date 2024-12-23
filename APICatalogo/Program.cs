using System.Reflection;
using System.Security.Claims;
using System.Text;
using APICatalogo.Context;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Logging;
using APICatalogo.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using APICatalogo.Models;
using APICatalogo.RateLimitOptions;
using APICatalogo.Services;
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
builder.Services.AddIdentity<ApplicationUser, IdentityRole>() //IdentityUser/ApplicationUser = Usuário | IdentityRole = funções do usuário
    .AddEntityFrameworkStores<AppDbContext>() //Adiciono Indentity para armazenar os dados tendo como base meu contexto
    .AddDefaultTokenProviders(); //tokens para lidar com a autenticação

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
var myOptions = new MyRateLimitOptions();
//Faz o Bind entre os valores padrões definidos no appsettings com os valores defaults definidos na classe MyRate:
builder.Configuration.GetSection(MyRateLimitOptions.MyRateLimit).Bind(myOptions);

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
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection)));

builder.Services.AddScoped<ApiLoggingFilter>();

// uma inst�ncia de CategoriaRepository ser� criada uma vez para cada escopo de request.
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();

//Registro do Repository gen�rico para poder acessar o banco por ele
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(typeof(ProdutoDTOMappingProfile));

//Registro no container DI do Service de Token
builder.Services.AddScoped<ITokenService, TokenService>();

// Adiciona o provedor de log personalizado (CustomLoggerProvider) ao sistema de log do ASP.NETCode, definindo o n�vel m�nimo de log como LogLevel.Information
builder.Logging.AddProvider(new CustomLoggerPrivider(new CustomLoggerProviderConfiguration
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
