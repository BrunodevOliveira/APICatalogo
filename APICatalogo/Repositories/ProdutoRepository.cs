using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;


namespace APICatalogo.Repositories;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{

    public ProdutoRepository(AppDbContext context) : base(context)
    {}

    public IEnumerable<Produto> GetProdutoPorCategoria(int id)
    {
        return GetAll().Where(c => c.CategoriaId == id);
    }

    //public IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams)
    //{
    //    return GetAll() //Traz todos os elementos
    //        .OrderBy(c => c.Nome)//Ordena por nome
    //        .Skip((produtosParams.PageNumber - 1) * produtosParams.PageSize) //Pula um número específico de elementos
    //        .Take(produtosParams.PageSize) // Retorna o número de elementos que serão exibidos
    //        .ToList();
    //}

    public PagedList<Produto> GetProdutos(ProdutosParameters produtosParams)
    {
        var produtos = GetAll().OrderBy(p => p.ProdutoId).AsQueryable(); //AsQueryable -> Converte e IEnumerable para IQueryable

        var produtosOrdenados = PagedList<Produto>.ToPagedList(produtos, produtosParams.PageNumber, produtosParams.PageSize);

        return produtosOrdenados;
    }

    public PagedList<Produto> GetProdutosFiltroPreco(ProdutosFiltroPreco produtosFiltroParams)
    {
        var produtos = GetAll().AsQueryable();

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

        var produtosFiltrados = PagedList<Produto>.ToPagedList(produtos, produtosFiltroParams.PageNumber, produtosFiltroParams.PageSize);

        return produtosFiltrados;
    }
}
