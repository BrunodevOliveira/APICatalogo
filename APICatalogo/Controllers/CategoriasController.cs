﻿using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers;
[Route("[controller]")]
[ApiController]
public class CategoriasController : ControllerBase
{
    private readonly IRepository<Categoria> _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;


    public CategoriasController(IConfiguration configuration, ILogger<CategoriasController> logger, IRepository<Categoria> repository)
    {
        _configuration = configuration;
        _logger = logger;
        _repository = repository;
    }

    [HttpGet("LerArquivoConfiguracao")]
    public string GetValoresConfig()
    {
        var valor1 = _configuration["chave1"];
        var valor2 = _configuration["chave2"];
        var secao1 = _configuration["secao1:chave2"];

        return valor1 + " " + valor2 + " " + secao1;
    }

    //[HttpGet("produtos")]
    //public ActionResult<IEnumerable<Categoria>> GetCategoriasProdutos()
    //{
    //    _logger.LogInformation("===============GET api/categorias/produtos================");
    //    //throw new DataMisalignedException();
    //    return _context.Categorias.Include(p => p.Produtos).ToList();
    //}


    [HttpGet]
    [ServiceFilter(typeof(ApiLoggingFilter))]
    public ActionResult<IEnumerable<Categoria>> Get()
    {
        var categorias = _repository.GetAll();

        return Ok(categorias);
    }


    [HttpGet("{id}", Name = "ObterCategoria")]
    public ActionResult<Categoria> Get(int id)
    {
        //throw new ArgumentException("Excecão para teste de Middleware");
        //throw new ArgumentException("Teste APIExceptionFilter - Ocorreu um erro no tratamento do request");

        _logger.LogInformation($"===============GET api/categorias/id = {id} ================");

        var categoria = _repository.Get(c => c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning($"Cateoria com id = {id} não encontrada");
            return NotFound($"Cateoria com id = {id} não encontrada");
        }

        return Ok(categoria);

    }

    [HttpPost]
    public ActionResult Post(Categoria categoria)
    {
        if (categoria is null)
        {
            _logger.LogWarning($"Dados invalidos: {categoria.ToString()}");
            return BadRequest("Falta informações..");
        }

        var categoriaCriada = _repository.Create(categoria);

        return new CreatedAtRouteResult("ObterCategoria", new { id = categoriaCriada.CategoriaId }, categoriaCriada);
    }

    [HttpPut("/Categorias/{id}")]
    public ActionResult<Categoria> Put(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId)
        {
            _logger.LogWarning("Dados inválidos.");
            return BadRequest();
        }

        var categoriaAtualizada = _repository.Update(categoria);

        return Ok(categoriaAtualizada);
    }

    [HttpDelete("/Categorias/{id}")]
    public ActionResult<Categoria> Delete(int id)
    {
        var categoria = _repository.Get(c => c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning("Categoria não localizada");
            return NotFound($"Categoria com id = {id} não localizada");
        }

        var categoriaExcluida = _repository.Delete(categoria);

        return Ok(categoriaExcluida);
    }
}