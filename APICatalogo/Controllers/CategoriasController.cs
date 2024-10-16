using APICatalogo.Context;
using APICatalogo.Filters;
using APICatalogo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers;
[Route("[controller]")]
[ApiController]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;


    public CategoriasController(AppDbContext context, IConfiguration configuration, ILogger<CategoriasController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("LerArquivoConfiguracao")]
    public string GetValoresConfig()
    {
        var valor1 = _configuration["chave1"];
        var valor2 = _configuration["chave2"];
        var secao1 = _configuration["secao1:chave2"];

        return valor1 + " " + valor2 + " " + secao1;
    }

    [HttpGet("produtos")]
    public ActionResult<IEnumerable<Categoria>> GetCategoriasProdutos()
    {
        _logger.LogInformation("===============GET api/categorias/produtos================");
        //throw new DataMisalignedException();
        return _context.Categorias.Include(p => p.Produtos).ToList();
    }


    [HttpGet]
    [ServiceFilter(typeof(ApiLoggingFilter))]
    public async Task<ActionResult<IEnumerable<Categoria>>> Get()
    {
        return await _context.Categorias.AsNoTracking().ToListAsync();
    }
    

    [HttpGet("{id}", Name = "ObterCategoria")]
    public ActionResult<Categoria> Get(int id)
    {
        //throw new ArgumentException("Excecão para teste de Middleware");
        //throw new ArgumentException("Teste APIExceptionFilter - Ocorreu um erro no tratamento do request");

        _logger.LogInformation($"===============GET api/categorias/id = {id} ================");
        var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
        if (categoria is null) return NotFound($"Cateoria com id = {id} não encontrada");

        return Ok(categoria);

    }

    [HttpPost]
    public ActionResult Post(Categoria categoria)
    {
        if (categoria is null) return BadRequest("Falta informações..");

        _context.Categorias.Add(categoria); //Adiciona o produto vindo do Body ao contexto do EF Core
        _context.SaveChanges(); //Persiste os dados na tabela

        //Retorna status 201 s um cabeçalho com o id e a rota para obter o produto criado
        return new CreatedAtRouteResult("ObterCategoria", new { id = categoria.CategoriaId }, categoria);
    }

    [HttpPut("/Categorias/{id}")]
    public ActionResult Put(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId) return BadRequest();

        _context.Entry(categoria).State = EntityState.Modified;//Informa ao contexto do EF Core que o objeto categoria foi modificado e precisa ser atualizado no BD
        _context.SaveChanges();

        return Ok(categoria);
    }

    [HttpDelete("/Categorias/{id}")]
    public ActionResult Delete(int id)
    {
        //Posso utilizar o método Find() quando id for PK para realizar a consulta de forma mais rápida pois ele busca em memória os valores
        var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);

        if (categoria is null) return NotFound("Categoria não localizada");
        _context.Categorias.Remove(categoria); // Remove a categoria do contexto do EF Core usando o método Remove
        _context.SaveChanges();

        return Ok(categoria);
    }
}