using APICatalogo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Context;

public class
    AppDbContext : IdentityDbContext<ApplicationUser> //DbContext => Representa uma sessão  com o banco de dados sendo a ponte entre as entidades de dominio e o banco
{
    //options => contem os parâmetros de configuração que serão utilizadas para configurar o contexto do banco de dados 
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    //Definir o mapeamento entre os Models e Tabelas 
    //DbSet<T> => Representa uma coleção de entidades no contexto que podem ser consultadas no banco de dados
    public DbSet<Categoria>? Categorias { get; set; }
    public DbSet<Produto>? Produtos { get; set; }

    //OnModelCreating informa que a classe do Identity deve trabalhar com uma instância de  ApplicationUser para representar usuários
    protected override void OnModelCreating(ModelBuilder builder)
    {
        //Invoca a implementação da classe base para garantir que as configurações padrões sejam aplicadas 
        base.OnModelCreating(builder); 
    }
}