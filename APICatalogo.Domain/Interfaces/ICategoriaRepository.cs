using APICatalogo.Domain.Entities;
using APICatalogo.Domain.Pagination;
using System.Threading.Tasks; // For Task
using X.PagedList;

namespace APICatalogo.Domain.Interfaces;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<IPagedList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParams);

    Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasParams);
}
