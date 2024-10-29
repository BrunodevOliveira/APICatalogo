using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<IEnumerable<Produto>> GetProdutoPorCategoriaAsync(int id);
    //IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams);
    Task<PagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParams);

    Task<PagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroPreco);
}
