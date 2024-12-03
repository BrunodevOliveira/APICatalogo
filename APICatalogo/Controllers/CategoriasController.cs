using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using X.PagedList;

namespace APICatalogo.Controllers;
[Route("[controller]")]
[ApiController]
[EnableRateLimiting("fixedwindow")]
public class CategoriasController : ControllerBase
{
    //private readonly IRepository<Categoria> _repository; Após implementa o padraão Unit of Work, não utilizo mais diretamente o repository
    private readonly  IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;


    public CategoriasController(IConfiguration configuration, ILogger<CategoriasController> logger, IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
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
    // [Authorize]
    [ServiceFilter(typeof(ApiLoggingFilter))]
    [DisableRateLimiting]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()
    {
        var categorias = await _unitOfWork.CategoriaRepository.GetAllAsync();

        if (categorias is null) return NotFound("Não existem categorias...");

        var categoriasDto = CategoriaDTOMappingExtensions.ToCategoriasDtoList(categorias);

        return Ok(categoriasDto);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetPagination([FromQuery]CategoriasParameters categoriasParameters)
    {
        var categorias = await _unitOfWork.CategoriaRepository.GetCategoriasAsync(categoriasParameters);
        return ObterCategorias(categorias);
    }

    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltradas([FromQuery] CategoriasFiltroNome categoriasFiltro)
    {
        var categorias = await _unitOfWork.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriasFiltro);

        return ObterCategorias(categorias);

    }

    private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(IPagedList<Categoria> categorias)
    {
        var metadata = new
        {
            categorias.Count,
            categorias.PageSize,
            categorias.PageCount,
            categorias.TotalItemCount,
            categorias.HasNextPage,
            categorias.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var categoriasDto = CategoriaDTOMappingExtensions.ToCategoriasDtoList(categorias);

        return Ok(categoriasDto);
    }

    [HttpGet("{id}", Name = "ObterCategoria")]
    public async Task<ActionResult<CategoriaDTO>> Get(int id)
    {
        //throw new ArgumentException("Excecão para teste de Middleware");
        //throw new ArgumentException("Teste APIExceptionFilter - Ocorreu um erro no tratamento do request");

        _logger.LogInformation($"===============GET api/categorias/id = {id} ================");

        var categoria = await _unitOfWork.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning($"Cateoria com id = {id} não encontrada");
            return NotFound($"Cateoria com id = {id} não encontrada");
        }

        var categoriaDto = CategoriaDTOMappingExtensions.ToCategoriaDto(categoria);

        return Ok(categoriaDto);

    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning($"Dados invalidos: {categoriaDto.ToString()}");
            return BadRequest("Falta informações..");
        }

        var categoria = CategoriaDTOMappingExtensions.ToCategoria(categoriaDto);

        var categoriaCriada = _unitOfWork.CategoriaRepository.Create(categoria);
        await _unitOfWork.CommitAsync();

        var novaCategoriaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoria);

        return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDTO.CategoriaId }, novaCategoriaDTO);
    }

    [HttpPut("/Categorias/{id}")]
    public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto)
    {   
        if (id != categoriaDto.CategoriaId)
        {
            _logger.LogWarning("Dados inválidos.");
            return BadRequest();
        }

        var categoria = CategoriaDTOMappingExtensions.ToCategoria(categoriaDto);

        // Quando fazemos Update(), o EF Core:
        // 1. Verifica se a entidade tem uma chave primária definida
        // 2. Marca o estado da entidade como "Modified"
        var categoriaAtualizada = _unitOfWork.CategoriaRepository.Update(categoria);

        // 1. Pega todas as entidades marcadas como "Modified"
        // 2. Gera o SQL UPDATE baseado na chave primária
        await _unitOfWork.CommitAsync();

        var categoriaAtualizadaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoria);

        return Ok(categoriaAtualizadaDTO);
    }

    [HttpDelete("/Categorias/{id}")]
    [Authorize(Roles = "AdminOnly")]
    public async  Task<ActionResult<CategoriaDTO>> Delete(int id)
    {
        var categoria = await _unitOfWork.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning("Categoria não localizada");
            return NotFound($"Categoria com id = {id} não localizada");
        }

        var categoriaExcluida = _unitOfWork.CategoriaRepository.Delete(categoria);

        await _unitOfWork.CommitAsync();

        var categoriaExcluidaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoriaExcluida);

        return Ok(categoriaExcluidaDTO);
    }
}