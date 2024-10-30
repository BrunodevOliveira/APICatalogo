using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;


namespace APICatalogo.Repositories;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{

    public ProdutoRepository(AppDbContext context) : base(context)
    {}

    public async  Task<IEnumerable<Produto>> GetProdutoPorCategoriaAsync(int id)
    {
        var produtos = await GetAllAsync();
        return produtos.Where(c => c.CategoriaId == id);
    }

    //public IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams)
    //{
    //    return GetAll() //Traz todos os elementos
    //        .OrderBy(c => c.Nome)//Ordena por nome
    //        .Skip((produtosParams.PageNumber - 1) * produtosParams.PageSize) //Pula um número específico de elementos
    //        .Take(produtosParams.PageSize) // Retorna o número de elementos que serão exibidos
    //        .ToList();
    //}

    public async Task<IPagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParams)
    {
        var produtos = await GetAllAsync();
        var produtosOrdenados = produtos.OrderBy(p => p.ProdutoId).AsQueryable(); //AsQueryable -> Converte e IEnumerable para IQueryable

       // var resultado = Pagination.PagedList<Produto>.ToPagedList(produtosOrdenados, produtosParams.PageNumber, produtosParams.PageSize);
       
       var resultado = await produtosOrdenados.ToPagedListAsync(produtosParams.PageNumber, produtosParams.PageSize);

        return resultado;
    }

    public async Task<IPagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParams)
    {
        var produtos = await GetAllAsync();

        var filtrosExistem = produtosFiltroParams.Preco.HasValue && !string.IsNullOrEmpty(produtosFiltroParams.PrecoCriterio);

         if (filtrosExistem)
         {
            if(produtosFiltroParams.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
            } else if(produtosFiltroParams.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco < produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
            } else if(produtosFiltroParams.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco == produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
            }
         }

        //var produtosFiltrados = Pagination.PagedList<Produto>.ToPagedList(produtos.AsQueryable(), produtosFiltroParams.PageNumber, produtosFiltroParams.PageSize);
        
        var produtosFiltrados = await produtos.ToPagedListAsync(produtosFiltroParams.PageNumber, 
            produtosFiltroParams.PageSize);

        return produtosFiltrados;
    }
}
