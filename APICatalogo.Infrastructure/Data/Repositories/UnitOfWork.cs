using APICatalogo.Domain.Interfaces; // For IUnitOfWork, IProdutoRepository, ICategoriaRepository
using APICatalogo.Infrastructure.Data; // For AppDbContext
using System.Threading.Tasks; // For Task

namespace APICatalogo.Infrastructure.Data.Repositories;

/*
     "Unit of Work" pattern:
        - As operações de escrita apenas preparam as mudanças
        - O SaveChanges é que realmente persiste tudo no banco
        - Apenas as operações que realmente acessam o banco são assíncronas
 */
public class UnitOfWork : IUnitOfWork // IDisposable is often implemented by UoW, but not explicitly here. AppDbContext is IDisposable.
{
    private IProdutoRepository? _produtoRepo;
    private ICategoriaRepository? _categoriaRepo;
    public AppDbContext _context; // Should ideally be private or protected readonly

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IProdutoRepository ProdutoRepository
    {
        get
        {
            // Injects AppDbContext instance into the repository constructor
            return _produtoRepo = _produtoRepo ?? new ProdutoRepository(_context);
        }
    }

    public ICategoriaRepository CategoriaRepository
    {
        get
        {
            // Injects AppDbContext instance into the repository constructor
            return _categoriaRepo = _categoriaRepo ?? new CategoriaRepository(_context);
        }
    }

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose() // This Dispose is part of IUnitOfWork from Domain, not necessarily System.IDisposable
    {
        _context.Dispose();
    }
}
