using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;


namespace APICatalogo.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(AppDbContext context) : base(context)
    {
    }

    public PagedList<Categoria> GetCategorias(CategoriasParameters categoriasParams)
    {
       var categorias = GetAll().OrderBy(c => c.CategoriaId).AsQueryable();

        var categoriasOrdenadas = PagedList<Categoria>
               .ToPagedList(categorias, categoriasParams.PageNumber, categoriasParams.PageSize);

        return categoriasOrdenadas;
    }

    public PagedList<Categoria> GetCategoriasFiltroNome(CategoriasFiltroNome categoriasParams)
    {
        var categorias = GetAll().AsQueryable();

        if(!string.IsNullOrEmpty(categoriasParams.Nome))
        {
            //Se não entrar nesse IF será retornado os valores de GetAll()
            categorias = categorias.Where(c => c.Nome.ToLower().Contains(categoriasParams.Nome.ToLower()));
        }

        var categortiasFiltradas = PagedList<Categoria>
              .ToPagedList(categorias, categoriasParams.PageNumber, categoriasParams.PageSize);

        return categortiasFiltradas;
    }
}
