using APICatalogo.Application.DTOs.Mappings; // Updated
using APICatalogo.Domain.Interfaces;          // Updated
using APICatalogo.Infrastructure.Data;        // Updated
using APICatalogo.Infrastructure.Data.Repositories; // Updated
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiCatalogoxUnitTests.UnitTests;

public class ProdutosUnitTestController //Usada para configurar os teste 
{
    public IUnitOfWork repository; // From Domain
    public IMapper mapper;
    public static DbContextOptions<AppDbContext> dbContextOptions; // AppDbContext from Infrastructure

    // Connection string should ideally come from configuration or be specific to test environment
    public static string connextionString = "Server=localhost;DataBase=CatalogoDB_Test;Uid=root;Pwd=3453"; // Using a test DB

    static ProdutosUnitTestController()
    {
        dbContextOptions = new DbContextOptionsBuilder<AppDbContext>() // AppDbContext from Infrastructure
            .UseMySql(connextionString, ServerVersion.AutoDetect(connextionString)) // Consider using InMemory for unit tests
            .Options;
    }

    public ProdutosUnitTestController()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ProdutoDTOMappingProfile()); // From Application
        });
        
        mapper = config.CreateMapper();
        var context = new AppDbContext(dbContextOptions); // AppDbContext from Infrastructure
        repository = new UnitOfWork(context); // UnitOfWork from Infrastructure
    }
}