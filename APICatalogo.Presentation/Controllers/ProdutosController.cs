using APICatalogo.Application.DTOs;       // Updated
using APICatalogo.Domain.Entities;          // Updated
using APICatalogo.Domain.Interfaces;        // Updated
using APICatalogo.Domain.Pagination;        // Updated
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;


namespace APICatalogo.Controllers;
[Route("[controller]")]
[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))] //Documenta no Swagger os possíveis status codes que uma Actionpode retornar
public class ProdutosController : ControllerBase
{
    //private readonly IProdutoRepository _produtoRepository; // Tem acesso aos métodos genéricos e específicos
    private readonly IUnitOfWork _unitOfWork; // Interface from Domain
    private readonly IMapper _mapper;

    public ProdutosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        //_produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("produtos/{id}")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosCategoria(int id)
    {
        var produtosPorCategoria = await _unitOfWork.ProdutoRepository.GetProdutoPorCategoriaAsync(id); // Produto from Domain

        if (produtosPorCategoria is null) return NotFound();

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtosPorCategoria);  // ProdutoDTO from Application, Produto from Domain

        return Ok(produtosDto); 
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetPagination([FromQuery] ProdutosParameters produtosParameters) // ProdutosParameters from Domain
    {
        var produtos = await _unitOfWork.ProdutoRepository.GetProdutosAsync(produtosParameters); // Produto from Domain

       return ObterProdutos(produtos); // Produto from Domain
    }

    [HttpGet("filter/preco/pagination")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFilterPreco([FromQuery] ProdutosFiltroPreco produtoFilterParameters) // ProdutosFiltroPreco from Domain
    {
        var produtos = await _unitOfWork.ProdutoRepository.GetProdutosFiltroPrecoAsync(produtoFilterParameters); // Produto from Domain

        return ObterProdutos(produtos); // Produto from Domain
    }

    private ActionResult<IEnumerable<ProdutoDTO>> ObterProdutos(IPagedList<Produto> produtos) // Produto from Domain, ProdutoDTO from Application
    {
        var metadata = new
        {
            produtos.Count,
            produtos.PageSize,
            produtos.PageCount,
            produtos.TotalItemCount,
            produtos.HasNextPage,
            produtos.HasPreviousPage
        };
        //Serializo os dados da paginação através da Lib NewtonsoftJson
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDto);
    }
    
    /// <summary>
    /// Obtem todos os produtos
    /// </summary>
    /// <returns>Lista de objetos produto</returns>
    [HttpGet]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get()
    {
        try
        {
            var produtos = await _unitOfWork.ProdutoRepository.GetAllAsync(); // Produto from Domain
            if (produtos is null) return NotFound();

            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos); // ProdutoDTO from Application, Produto from Domain

            return Ok(produtosDto);
        }
        catch (Exception) // Removed 'e' as it's not used
        {
            return BadRequest();
        }
    }

    [HttpGet("{id}", Name= "ObterProduto")]
    public async Task<ActionResult<ProdutoDTO>> Get(int id) //[FromQuery] int id -> Dessa forma não preciso passar o ID como parâmetro da Action.
    {
        var produto = await _unitOfWork.ProdutoRepository.GetAsync(p => p.ProdutoId == id); // Produto from Domain
        
        if(id <= 0) return BadRequest("Id de produto inválido");
        if (produto is null) return NotFound("Produto não encontrado");

        var produtoDto = _mapper.Map<ProdutoDTO>(produto); // ProdutoDTO from Application, Produto from Domain

        return Ok(produtoDto);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDTO>> Post([FromBody] ProdutoDTO produtoDto) // ProdutoDTO from Application
    {
        if (produtoDto is null) return BadRequest("Falta informações..");

        var produto = _mapper.Map<Produto>(produtoDto); // Produto from Domain, ProdutoDTO from Application

        var novoProduto = _unitOfWork.ProdutoRepository.Create(produto); // Produto from Domain
        await _unitOfWork.CommitAsync();

        var novoProdutoDto = _mapper.Map<ProdutoDTO>(novoProduto); // ProdutoDTO from Application, Produto from Domain

        //Retorna status 201 s um cabeçalho com o id e a rota para obter o produto criado
        return new CreatedAtRouteResult("ObterProduto", new { id = novoProdutoDto.ProdutoId }, novoProdutoDto); 
    }

    [HttpPatch("{id}/UpdatePartial")]
    public async  Task<ActionResult<ProdutoDTOUpdateResponse>> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDto) // ProdutoDTOUpdateRequest, ProdutoDTOUpdateResponse from Application
    {
        if (patchProdutoDto is null || id <= 0) return BadRequest();

        var produto = await _unitOfWork.ProdutoRepository.GetAsync(c => c.ProdutoId == id); // Produto from Domain

        if(produto is null) return NotFound();

        var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto); // ProdutoDTOUpdateRequest from Application, Produto from Domain

        //Aplica as atualizaçoes no produto e caso haja erro será armazenado no ModelState
        patchProdutoDto.ApplyTo(produtoUpdateRequest, ModelState);

        /*
            TryValidateModel() executa a validação e adiciona quaisquer erros ao ModelState. TryValidateModel() retorna true caso esteja valido
            Em seguida, verificamos se o ModelState é válido
            Se não for válido, retornamos BadRequest com os erros
         */
        if (!TryValidateModel(produtoUpdateRequest)) // Simplified conditional
        {
            return BadRequest(ModelState);
        }
        // Mapeio novamente o objeto produtoUpdateRequest para Produto
        _mapper.Map(produtoUpdateRequest, produto); // ProdutoDTOUpdateRequest from Application, Produto from Domain

        //Atualizo no DB
        _unitOfWork.ProdutoRepository.Update(produto); // Produto from Domain
        await _unitOfWork.CommitAsync();

        //Retorno o DTO
        return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto)); // ProdutoDTOUpdateResponse from Application, Produto from Domain
    }


    [HttpPut("/Produtos/{id}")]
    public async Task<ActionResult<ProdutoDTO>> Put(int id, ProdutoDTO produtoDto) { // ProdutoDTO from Application
        if (id != produtoDto.ProdutoId) return BadRequest();

        var produto = _mapper.Map<Produto>(produtoDto); // Produto from Domain, ProdutoDTO from Application

        // Quando fazemos Update(), o EF Core:
        // 1. Verifica se a entidade tem uma chave primária definida
        // 2. Marca o estado da entidade como "Modified"
        var atualizouProduto = _unitOfWork.ProdutoRepository.Update(produto); // Produto from Domain

        // 1. Pega todas as entidades marcadas como "Modified"
        // 2. Gera o SQL UPDATE baseado na chave primária
        await _unitOfWork.CommitAsync();

        var novoProdutoDto = _mapper.Map<ProdutoDTO>(atualizouProduto); // ProdutoDTO from Application, Produto from Domain

        return atualizouProduto is not null ? Ok(novoProdutoDto) 
            : StatusCode(500, $"Falha ao atualizar o produto de id = {id}");
    }

    [HttpDelete("/Produtos/{id}")]
    public async  Task<ActionResult<ProdutoDTO>> Delete(int id)
    {
        var produto = await _unitOfWork.ProdutoRepository.GetAsync(p =>p.ProdutoId == id); // Produto from Domain

        if(produto is null)
            return NotFound($"Produto com id = {id} não localizado");

        var produtoExcluido = _unitOfWork.ProdutoRepository.Delete(produto); // Produto from Domain


        await _unitOfWork.CommitAsync();

        var produtoExcluidoDto = _mapper.Map<ProdutoDTO>(produtoExcluido); // ProdutoDTO from Application, Produto from Domain

        return Ok(produtoExcluidoDto);

    }
}
