using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;

namespace APICatalogo.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<IEnumerable<Produto>> GetProdutoPorCategoriaAsync(int id);
    //IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams);
    Task<IPagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParams);

    Task<IPagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroPreco);
}
