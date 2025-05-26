using APICatalogo.Domain.Entities;
using APICatalogo.Domain.Interfaces;
using APICatalogo.Domain.Pagination;
using APICatalogo.Infrastructure.Data; // For AppDbContext
using System; // For StringComparison
using System.Collections.Generic; // For IEnumerable
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
// using Microsoft.EntityFrameworkCore; // Might be needed for ToPagedListAsync

namespace APICatalogo.Infrastructure.Data.Repositories;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{

    public ProdutoRepository(AppDbContext context) : base(context)
    {}

    public async  Task<IEnumerable<Produto>> GetProdutoPorCategoriaAsync(int id)
    {
        var produtos = await GetAllAsync();
        return produtos.Where(c => c.CategoriaId == id);
    }

    public async Task<IPagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParams)
    {
        var produtos = await GetAllAsync();
        var produtosOrdenados = produtos.OrderBy(p => p.ProdutoId).AsQueryable(); 
       
       var resultado = await produtosOrdenados.ToPagedListAsync(produtosParams.PageNumber, produtosParams.PageSize);

        return resultado;
    }

    public async Task<IPagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParams)
    {
        var produtos = await GetAllAsync();

        // Ensure PrecoCriterio is not null before calling Equals on it.
        var filtrosExistem = produtosFiltroParams.Preco.HasValue && 
                             !string.IsNullOrEmpty(produtosFiltroParams.PrecoCriterio);

         if (filtrosExistem && produtosFiltroParams.PrecoCriterio != null)
         {
            if(produtosFiltroParams.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco!.Value).OrderBy(p => p.Preco);
            } else if(produtosFiltroParams.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco < produtosFiltroParams.Preco!.Value).OrderBy(p => p.Preco);
            } else if(produtosFiltroParams.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco == produtosFiltroParams.Preco!.Value).OrderBy(p => p.Preco);
            }
         }
        
        var produtosFiltrados = await produtos.AsQueryable().ToPagedListAsync(produtosFiltroParams.PageNumber, // Added AsQueryable() for safety
            produtosFiltroParams.PageSize);

        return produtosFiltrados;
    }
}
