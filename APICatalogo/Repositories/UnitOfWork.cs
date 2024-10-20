using APICatalogo.Context;

namespace APICatalogo.Repositories;

public class UnitOfWork : IUnitOfWork
{
    /*
        - Não injetamos _produtoRepo e  _categoriaRepo no construtor pois não queremos criar uma nova instância 
            sempre que a classe UnitOfWork for chamada
     */
    private IProdutoRepository? _produtoRepo;

    private ICategoriaRepository? _categoriaRepo;

    public AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /*
        - Obtem uma instância de produto repository
        - Só criamos uma instância dos repositories caso não exista
        - Abordagem lazy Loading -> adicar a obtenção dos objetos até que eles sejam realmente necessários
     */
    public IProdutoRepository ProdutoRepository
    {
        get
        {
            return _produtoRepo = _produtoRepo ?? new ProdutoRepository(_context);
        }
    }

    public ICategoriaRepository CategoriaRepository
    {
        get
        {
            return _categoriaRepo = _categoriaRepo ?? new CategoriaRepository(_context);
        }
    }

    public void Commit()
    {
        _context.SaveChanges();
    }

    public void Dispose()
    {
        //Desaloca recursos
        _context.Dispose();
    }
} 
