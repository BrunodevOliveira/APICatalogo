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

    public IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams)
    {
        /*
            Para a página 1, PageNumber é 1. Então (1 - 1) * 10 é 0, logo Skip(0) não pula nenhum item e Take(10) pega os primeiros 10 itens.

            Para a página 2, PageNumber é 2. Então (2 - 1) * 10 é 10, logo Skip(10) pula os primeiros 10 itens e Take(10) pega os próximos 10 itens.

            E assim por diante.
         */

        return GetAll() //Traz todos os elementos
            .OrderBy(c => c.Nome)//Ordena por nome
            .Skip((produtosParams.PageNumber - 1) * produtosParams.PageSize) //Pula um número específico de elementos
            .Take(produtosParams.PageSize) // Retorna o número de elementos que serão exibidos
            .ToList();
    }
}
