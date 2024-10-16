using APICatalogo.Context;
using APICatalogo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers;
[Route("[controller]")]
[ApiController]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProdutosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produto>>> Get()
    {
        /*
            AsNoTracking() é uma ferramenta poderosa para otimizar a performance de consultas em aplicações .NET
            que utilizam Entity Framework, especialmente quando essas consultas são destinadas apenas à leitura dos dados.
         */
        var produtos = await _context.Produtos.AsNoTracking().ToListAsync();
        if(produtos is null) return NotFound("Produtos não encontrados");
        return produtos;
    }

    [HttpGet("{id}", Name= "ObterProduto")]
    public ActionResult<Produto> Get([FromQuery] int id)
    {
        var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id);
        if (produto is null) return NotFound("Produto não encontrado");
        return produto;
    }

    [HttpPost]
    public ActionResult Post([FromBody] Produto produto)
    {
        if (produto is null) return BadRequest("Falta informações..");

        _context.Produtos.Add(produto); //Adiciona o produto vindo do Body no contexto
        _context.SaveChanges(); //Persiste os dados na tabela 

        //Retorna status 201 s um cabeçalho com o id e a rota para obter o produto criado
        return new CreatedAtRouteResult("ObterProduto", new { id = produto.ProdutoId }, produto); 
    }

    [HttpPut("/Produtos/{id}")]
    public ActionResult Put(int id, Produto produto) {
        if (id != produto.ProdutoId) return BadRequest();

        _context.Entry(produto).State = EntityState.Modified; //Atualiza todo o Produto
        _context.SaveChanges();

        return Ok(produto);
    }

    [HttpDelete("/Produtos/{id}")]
    public ActionResult Delete(int id)
    {
        var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id);

        if (produto is null) return NotFound("Produto não localizado");

        _context.Produtos.Remove(produto);
        _context.SaveChanges();

        return Ok(produto);
    }
}
