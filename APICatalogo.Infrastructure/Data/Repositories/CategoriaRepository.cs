using APICatalogo.Domain.Entities;
using APICatalogo.Domain.Interfaces;
using APICatalogo.Domain.Pagination;
using APICatalogo.Infrastructure.Data; // For AppDbContext
using System.Linq;
using System.Threading.Tasks;
using X.PagedList; // Assuming ToPagedListAsync is from here or an EF Core extension.
// using Microsoft.EntityFrameworkCore; // Might be needed for ToPagedListAsync if it's an EF Core specific extension from X.PagedList.EntityFrameworkCore

namespace APICatalogo.Infrastructure.Data.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IPagedList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParams)
    {
        var categorias = await GetAllAsync();
        // OrderBy requires System.Linq. IQueryable is also from System.Linq.
        var categoriasOrdenadas = categorias.OrderBy(c => c.CategoriaId).AsQueryable(); 
        
        // ToPagedListAsync is typically part of X.PagedList.EntityFrameworkCore or similar for IQueryable.
        // If GetAllAsync() returns IEnumerable, it might need .AsQueryable() before ToPagedListAsync if that extension expects IQueryable.
        // For now, assuming X.PagedList provides a suitable ToPagedListAsync for IEnumerable or IQueryable.
        var resultado = await categoriasOrdenadas.ToPagedListAsync(categoriasParams.PageNumber, 
            categoriasParams.PageSize);

        return resultado;
    }

    public async Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasParams)
    {
        var categorias = await GetAllAsync();

        if (!string.IsNullOrEmpty(categoriasParams.Nome))
        {
            categorias = categorias.Where(c => c.Nome != null && c.Nome.ToLower().Contains(categoriasParams.Nome.ToLower()));
        }
        
        // Assuming ToPagedListAsync works on IEnumerable or IQueryable from X.PagedList
        var categortiasFiltradas =  await categorias.AsQueryable().ToPagedListAsync(categoriasParams.PageNumber, // Added AsQueryable() for safety
            categoriasParams.PageSize);
          
        return categortiasFiltradas;
    }
}
