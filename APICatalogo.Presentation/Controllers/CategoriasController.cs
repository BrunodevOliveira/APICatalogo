using APICatalogo.Application.DTOs;             // Updated
using APICatalogo.Application.DTOs.Mappings;    // Updated
using APICatalogo.Domain.Entities;            // Updated
using APICatalogo.Domain.Interfaces;          // Updated
using APICatalogo.Domain.Pagination;          // Updated
using APICatalogo.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using X.PagedList;

namespace APICatalogo.Controllers;
[Route("[controller]")]
[ApiController]
[EnableRateLimiting("fixedwindow")]
[Produces("application/json")] //Actions só irão retornar JSON
public class CategoriasController : ControllerBase
{
    //private readonly IRepository<Categoria> _repository; Após implementa o padraão Unit of Work, não utilizo mais diretamente o repository
    private readonly  IUnitOfWork _unitOfWork; // Interface from Domain
    private readonly IConfiguration _configuration;
    private readonly ILogger<CategoriasController> _logger; // Typed logger


    public CategoriasController(IConfiguration configuration, ILogger<CategoriasController> logger, IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _logger = logger; // Correctly typed
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

    /// <summary>
    /// Obtem uma lista de categorias
    /// </summary>
    /// <returns>Objetos categoria</returns>
    [HttpGet]
    // [Authorize]
    [ServiceFilter(typeof(ApiLoggingFilter))]
    [DisableRateLimiting]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()
    {
        var categorias = await _unitOfWork.CategoriaRepository.GetAllAsync(); // Categoria from Domain

        if (categorias is null) return NotFound("Não existem categorias...");

        var categoriasDto = CategoriaDTOMappingExtensions.ToCategoriasDtoList(categorias); // CategoriaDTO from Application, Categoria from Domain

        return Ok(categoriasDto);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetPagination([FromQuery]CategoriasParameters categoriasParameters) // CategoriasParameters from Domain
    {
        var categorias = await _unitOfWork.CategoriaRepository.GetCategoriasAsync(categoriasParameters); // Categoria from Domain
        return ObterCategorias(categorias); // Categoria from Domain
    }

    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltradas([FromQuery] CategoriasFiltroNome categoriasFiltro) // CategoriasFiltroNome from Domain
    {
        var categorias = await _unitOfWork.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriasFiltro); // Categoria from Domain

        return ObterCategorias(categorias); // Categoria from Domain

    }

    private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(IPagedList<Categoria> categorias) // Categoria from Domain, CategoriaDTO from Application
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
    
    /// <summary>
    /// Obtem uma categoria por id
    /// </summary>
    /// <param name="id">Código da categoria</param>
    /// <returns>Objetos categoria</returns>
    [HttpGet("{id}", Name = "ObterCategoria")]
    public async Task<ActionResult<CategoriaDTO>> Get(int id)
    {
        //throw new ArgumentException("Excecão para teste de Middleware");
        //throw new ArgumentException("Teste APIExceptionFilter - Ocorreu um erro no tratamento do request");

        _logger.LogInformation($"===============GET api/categorias/id = {id} ================");

        var categoria = await _unitOfWork.CategoriaRepository.GetAsync(c => c.CategoriaId == id); // Categoria from Domain

        if (categoria is null)
        {
            _logger.LogWarning($"Cateoria com id = {id} não encontrada");
            return NotFound($"Cateoria com id = {id} não encontrada");
        }

        var categoriaDto = CategoriaDTOMappingExtensions.ToCategoriaDto(categoria); // Categoria from Domain, CategoriaDTO from Application

        return Ok(categoriaDto);

    }
    /// <summary>
    /// Inclui uma nova categoria
    /// </summary>
    /// <remarks>
    ///Exemplo de request:
    /// POST api/categorias
    /// {
    /// "categoriaId": 1,
    /// "nome": "CategoriaName",
    /// "imagemUrl": "http://google.com"
    /// }
    /// </remarks>
    /// <param name="categoriaDto">Objeto categoria</param> // CategoriaDTO from Application
    /// <returns>Objetos categoria</returns>
    /// <remarks>Retorna um objeto categoria incluído</remarks>
    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto) // CategoriaDTO from Application
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning($"Dados invalidos: {categoriaDto.ToString()}");
            return BadRequest("Falta informações..");
        }

        var categoria = CategoriaDTOMappingExtensions.ToCategoria(categoriaDto); // Categoria from Domain, CategoriaDTO from Application

        var categoriaCriada = _unitOfWork.CategoriaRepository.Create(categoria!); // Added null-forgiving
        await _unitOfWork.CommitAsync();

        var novaCategoriaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoriaCriada); // Categoria from Domain, CategoriaDTO from Application

        return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDTO!.CategoriaId }, novaCategoriaDTO); // null-forgiving was already there
    }

    [HttpPut("/Categorias/{id}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
    public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto) // CategoriaDTO from Application
    {   
        if (id != categoriaDto.CategoriaId)
        {
            _logger.LogWarning("Dados inválidos.");
            return BadRequest();
        }

        var categoria = CategoriaDTOMappingExtensions.ToCategoria(categoriaDto); // Categoria from Domain, CategoriaDTO from Application

        // Quando fazemos Update(), o EF Core:
        // 1. Verifica se a entidade tem uma chave primária definida
        // 2. Marca o estado da entidade como "Modified"
        var categoriaAtualizada = _unitOfWork.CategoriaRepository.Update(categoria!); // Added null-forgiving

        // 1. Pega todas as entidades marcadas como "Modified"
        // 2. Gera o SQL UPDATE baseado na chave primária
        await _unitOfWork.CommitAsync();

        var categoriaAtualizadaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoriaAtualizada); // Categoria from Domain, CategoriaDTO from Application

        return Ok(categoriaAtualizadaDTO);
    }

    [HttpDelete("/Categorias/{id}")]
    [Authorize(Roles = "AdminOnly")]
    public async  Task<ActionResult<CategoriaDTO>> Delete(int id)
    {
        var categoria = await _unitOfWork.CategoriaRepository.GetAsync(c => c.CategoriaId == id); // Categoria from Domain

        if (categoria is null)
        {
            _logger.LogWarning("Categoria não localizada");
            return NotFound($"Categoria com id = {id} não localizada");
        }

        var categoriaExcluida = _unitOfWork.CategoriaRepository.Delete(categoria); // Categoria from Domain

        await _unitOfWork.CommitAsync();

        var categoriaExcluidaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoriaExcluida); // Categoria from Domain, CategoriaDTO from Application

        return Ok(categoriaExcluidaDTO);
    }
}