using APICatalogo.Domain.Entities;
using APICatalogo.Domain.Pagination;
using System.Collections.Generic; // For IEnumerable
using System.Threading.Tasks; // For Task
using X.PagedList;

namespace APICatalogo.Domain.Interfaces;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<IEnumerable<Produto>> GetProdutoPorCategoriaAsync(int id);
    //IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams);
    Task<IPagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParams);

    Task<IPagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroPreco);
}
