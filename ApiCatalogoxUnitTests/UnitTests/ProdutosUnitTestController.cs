using APICatalogo.Context;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiCatalogoxUnitTests.UnitTests;

public class ProdutosUnitTestController //Usada para configurar os teste 
{
    public IUnitOfWork repository;
    public IMapper mapper;
    public static DbContextOptions<AppDbContext> dbContextOptions;

    public static string connextionString = "Server=localhost;DataBase=CatalogoDB;Uid=root;Pwd=3453";

    //Configura a variável  dbContextOptions. é executado apenas uma vez ao instanciar a classe.
    static ProdutosUnitTestController()
    {
        dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connextionString, ServerVersion.AutoDetect(connextionString))
            .Options;
    }

    public ProdutosUnitTestController()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ProdutoDTOMappingProfile());
        });
        
        mapper = config.CreateMapper();
        var context = new AppDbContext(dbContextOptions);
        repository = new UnitOfWork(context);
    }
}