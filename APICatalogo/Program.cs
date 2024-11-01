using System.Text;
using APICatalogo.Context;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Logging;
using APICatalogo.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using APICatalogo.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var secretKey = builder.Configuration["JWT:SecretKey"] ?? 
            throw new ArgumentNullException("Invalid secret Key!");

// Add services to the container.

builder.Services
    .AddControllers(options => options.Filters.Add(typeof(APIExceptionFilter)))
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
    .AddNewtonsoftJson();               

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configuração do JWT Token:
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer();

//Configuração do Indentity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>() //IdentityUser/ApplicationUser = Usuário | IdentityRole = funções do usuário
    .AddEntityFrameworkStores<AppDbContext>() //Adiciono Indentity para armazenar os dados tendo como base meu contexto
    .AddDefaultTokenProviders(); //tokens para lidar com a autenticação

//Habilitando e configurando a autenticação JWT Bearer
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

string mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");

//Registro do EF no container DI nativo usando o m�todo AddDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection)));

builder.Services.AddScoped<ApiLoggingFilter>();

// uma inst�ncia de CategoriaRepository ser� criada uma vez para cada escopo de request.
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();

//Registro do Repository gen�rico para podet acessar o banco por ele
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(typeof(ProdutoDTOMappingProfile));


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
