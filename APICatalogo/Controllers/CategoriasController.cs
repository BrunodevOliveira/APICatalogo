using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers;
[Route("[controller]")]
[ApiController]
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
    [ServiceFilter(typeof(ApiLoggingFilter))]
    public ActionResult<IEnumerable<CategoriaDTO>> Get()
    {
        var categorias = _unitOfWork.CategoriaRepository.GetAll();

        if (categorias is null) return NotFound("Não existem categorias...");

        var categoriasDto = CategoriaDTOMappingExtensions.ToCategoriasDtoList(categorias);

        return Ok(categoriasDto);
    }


    [HttpGet("{id}", Name = "ObterCategoria")]
    public ActionResult<CategoriaDTO> Get(int id)
    {
        //throw new ArgumentException("Excecão para teste de Middleware");
        //throw new ArgumentException("Teste APIExceptionFilter - Ocorreu um erro no tratamento do request");

        _logger.LogInformation($"===============GET api/categorias/id = {id} ================");

        var categoria = _unitOfWork.CategoriaRepository.Get(c => c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning($"Cateoria com id = {id} não encontrada");
            return NotFound($"Cateoria com id = {id} não encontrada");
        }

        var categoriaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoria);

        return Ok(categoriaDTO);

    }

    [HttpPost]
    public ActionResult<CategoriaDTO> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning($"Dados invalidos: {categoriaDto.ToString()}");
            return BadRequest("Falta informações..");
        }

        var categoria = CategoriaDTOMappingExtensions.ToCategoria(categoriaDto);

        var categoriaCriada = _unitOfWork.CategoriaRepository.Create(categoria);
        _unitOfWork.Commit();

        var novaCategoriaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoria);

        return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDTO.CategoriaId }, novaCategoriaDTO);
    }

    [HttpPut("/Categorias/{id}")]
    public ActionResult<CategoriaDTO> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
        {
            _logger.LogWarning("Dados inválidos.");
            return BadRequest();
        }

        var categoria = CategoriaDTOMappingExtensions.ToCategoria(categoriaDto);

        var categoriaAtualizada = _unitOfWork.CategoriaRepository.Update(categoria);
        _unitOfWork.Commit();

        var categoriaAtualizadaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoria);

        return Ok(categoriaAtualizadaDTO);
    }

    [HttpDelete("/Categorias/{id}")]
    public ActionResult<CategoriaDTO> Delete(int id)
    {
        var categoria = _unitOfWork.CategoriaRepository.Get(c => c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning("Categoria não localizada");
            return NotFound($"Categoria com id = {id} não localizada");
        }

        var categoriaExcluida = _unitOfWork.CategoriaRepository.Delete(categoria);

        _unitOfWork.Commit();

        var categoriaExcluidaDTO = CategoriaDTOMappingExtensions.ToCategoriaDto(categoriaExcluida);

        return Ok(categoriaExcluidaDTO);
    }
}