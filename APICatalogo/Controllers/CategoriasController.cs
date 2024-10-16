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
        try
        {
            _logger.LogInformation("===============GET api/categorias/produtos================");
            //throw new DataMisalignedException();
            return _context.Categorias.Include(p => p.Produtos).ToList();
        }
        catch (Exception)
        {

            return StatusCode(StatusCodes.Status500InternalServerError, 
                "Problema ao tratar sua solicitação.");
        }
    }
    [HttpGet]
    [ServiceFilter(typeof(ApiLoggingFilter))]
    public async Task<ActionResult<IEnumerable<Categoria>>> Get()
    {
        try
        {
            return await _context.Categorias.AsNoTracking().ToListAsync();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                 "Problema ao tratar sua solicitação.");
        }
    }
    

    [HttpGet("{id}", Name = "ObterCategoria")]
    public ActionResult<Categoria> Get(int id)
    {
        //throw new Exception("Excecão para teste de Middleware");
        try
        {
            _logger.LogInformation($"===============GET api/categorias/id = {id} ================");
            var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
            if (categoria is null) return NotFound($"Cateoria com id = {id} não encontrada");

            return Ok(categoria);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "Problema ao tratar sua solicitação.");
        }
    }

    [HttpPost]
    public ActionResult Post(Categoria categoria)
    {
        if (categoria is null) return BadRequest("Falta informações..");

        _context.Categorias.Add(categoria); //Adiciona o produto vindo do Body no contexto
        _context.SaveChanges(); //Persiste os dados na tabela

        //Retorna status 201 s um cabeçalho com o id e a rota para obter o produto criado
        return new CreatedAtRouteResult("ObterCategoria", new { id = categoria.CategoriaId }, categoria);
    }

    [HttpPut("/Categorias/{id}")]
    public ActionResult Put(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId) return BadRequest();

        _context.Entry(categoria).State = EntityState.Modified; //Atualiza todo o Produto
        _context.SaveChanges();

        return Ok(categoria);
    }

    [HttpDelete("/Categorias/{id}")]
    public ActionResult Delete(int id)
    {
        var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);

        if (categoria is null) return NotFound("Categoria não localizada");
        _context.Categorias.Remove(categoria);
        _context.SaveChanges();

        return Ok(categoria);
    }
}