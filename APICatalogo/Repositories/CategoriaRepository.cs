using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;


namespace APICatalogo.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IPagedList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParams)
    {
        var categorias = await GetAllAsync();
        var categoriasOrdenadas = categorias.OrderBy(c => c.CategoriaId).AsQueryable();

        //var resultado = Pagination.PagedList<Categoria>
          //     .ToPagedList(categoriasOrdenadas, categoriasParams.PageNumber, categoriasParams.PageSize);
        
          var resultado = await categoriasOrdenadas.ToPagedListAsync(categoriasParams.PageNumber, 
              categoriasParams.PageSize);

        return resultado;
    }

    public async Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasParams)
    {
        var categorias = await GetAllAsync();


        if (!string.IsNullOrEmpty(categoriasParams.Nome))
        {
            //Se não entrar nesse IF será retornado os valores de GetAll()
            categorias = categorias.Where(c => c.Nome.ToLower().Contains(categoriasParams.Nome.ToLower()));
        }

        //var categortiasFiltradas = Pagination.PagedList<Categoria>
          //    .ToPagedList(categorias.AsQueryable(), categoriasParams.PageNumber, categoriasParams.PageSize);

          var categortiasFiltradas =  await categorias.ToPagedListAsync(categoriasParams.PageNumber, 
              categoriasParams.PageSize);
          
        return categortiasFiltradas;
    }
}
