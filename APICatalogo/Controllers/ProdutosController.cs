using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers;
[Route("[controller]")]
[ApiController]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoRepository _produtoRepository; // Tem acesso aos métodos genéricos e específicos

    public ProdutosController(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    [HttpGet("produtos/{id}")]
    public ActionResult<IEnumerable<Produto>> GetProdutosCategoria(int id)
    {
        var produtosPorCategoria = _produtoRepository.GetProdutoPorCategoria(id);

        if (produtosPorCategoria is null) return NotFound();

        return Ok(produtosPorCategoria); 
    }

    [HttpGet]
    public ActionResult<IEnumerable<Produto>> Get()
    {
        var produtos = _produtoRepository.GetAll();
        if (produtos is null) return NotFound();

        return Ok(produtos);
    }

    [HttpGet("{id}", Name= "ObterProduto")]
    public ActionResult<Produto> Get(int id) //[FromQuery] int id -> Dessa forma não preciso passar o ID como parâmetro da Action.
    {
        var produto = _produtoRepository.Get(p => p.ProdutoId == id);
        if (produto is null) return NotFound("Produto não encontrado");
        return produto;
    }

    [HttpPost]
    public ActionResult Post([FromBody] Produto produto)
    {
        if (produto is null) return BadRequest("Falta informações..");

        var novoProduto = _produtoRepository.Create(produto);

        //Retorna status 201 s um cabeçalho com o id e a rota para obter o produto criado
        return new CreatedAtRouteResult("ObterProduto", new { id = novoProduto.ProdutoId }, novoProduto); 
    }

    [HttpPut("/Produtos/{id}")]
    public ActionResult Put(int id, Produto produto) {
        if (id != produto.ProdutoId) return BadRequest();

        var atualizouProduto = _produtoRepository.Update(produto);

        return atualizouProduto is not null ? Ok(produto) 
            : StatusCode(500, $"Falha ao atualizar o produto de id = {id}");
    }

    [HttpDelete("/Produtos/{id}")]
    public ActionResult Delete(int id)
    {
        var produto = _produtoRepository.Get(p =>p.ProdutoId == id);

        if(produto is null)
            return NotFound($"Produto com id = {id} não localizado");

        var produtoExcluido = _produtoRepository.Delete(produto);

        return Ok(produtoExcluido);

    }
}
