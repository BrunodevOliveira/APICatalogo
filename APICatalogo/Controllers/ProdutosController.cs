using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers;
[Route("[controller]")]
[ApiController]
public class ProdutosController : ControllerBase
{
    //private readonly IProdutoRepository _produtoRepository; // Tem acesso aos métodos genéricos e específicos
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProdutosController(IProdutoRepository produtoRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        //_produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("produtos/{id}")]
    public ActionResult<IEnumerable<ProdutoDTO>> GetProdutosCategoria(int id)
    {
        var produtosPorCategoria = _unitOfWork.ProdutoRepository.GetProdutoPorCategoria(id);

        if (produtosPorCategoria is null) return NotFound();

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtosPorCategoria);  //Map<Destino>(Origem)

        return Ok(produtosDto); 
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProdutoDTO>> Get()
    {
        var produtos = _unitOfWork.ProdutoRepository.GetAll();
        if (produtos is null) return NotFound();

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDto);
    }

    [HttpGet("{id}", Name= "ObterProduto")]
    public ActionResult<ProdutoDTO> Get(int id) //[FromQuery] int id -> Dessa forma não preciso passar o ID como parâmetro da Action.
    {
        var produto = _unitOfWork.ProdutoRepository.Get(p => p.ProdutoId == id);
        if (produto is null) return NotFound("Produto não encontrado");

        var produtoDto = _mapper.Map<ProdutoDTO>(produto);

        return Ok(produtoDto);
    }

    [HttpPost]
    public ActionResult<ProdutoDTO> Post([FromBody] ProdutoDTO produtoDto)
    {
        if (produtoDto is null) return BadRequest("Falta informações..");

        var produto = _mapper.Map<Produto>(produtoDto);

        var novoProduto = _unitOfWork.ProdutoRepository.Create(produto);
        _unitOfWork.Commit();

        var novoProdutoDto = _mapper.Map<ProdutoDTO>(novoProduto);

        //Retorna status 201 s um cabeçalho com o id e a rota para obter o produto criado
        return new CreatedAtRouteResult("ObterProduto", new { id = novoProdutoDto.ProdutoId }, novoProdutoDto); 
    }

    [HttpPut("/Produtos/{id}")]
    public ActionResult<ProdutoDTO> Put(int id, ProdutoDTO produtoDto) {
        if (id != produtoDto.ProdutoId) return BadRequest();

        var produto = _mapper.Map<Produto>(produtoDto);

        var atualizouProduto = _unitOfWork.ProdutoRepository.Update(produto);
        _unitOfWork.Commit();

        var novoProdutoDto = _mapper.Map<ProdutoDTO>(atualizouProduto);

        return atualizouProduto is not null ? Ok(novoProdutoDto) 
            : StatusCode(500, $"Falha ao atualizar o produto de id = {id}");
    }

    [HttpDelete("/Produtos/{id}")]
    public ActionResult<ProdutoDTO> Delete(int id)
    {
        var produto = _unitOfWork.ProdutoRepository.Get(p =>p.ProdutoId == id);

        if(produto is null)
            return NotFound($"Produto com id = {id} não localizado");

        var produtoExcluido = _unitOfWork.ProdutoRepository.Delete(produto);
        _unitOfWork.Commit();

        var produtoExcluidoDto = _mapper.Map<ProdutoDTO>(produtoExcluido);

        return Ok(produtoExcluidoDto);

    }
}
