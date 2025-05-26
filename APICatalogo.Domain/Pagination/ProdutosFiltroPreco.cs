namespace APICatalogo.Domain.Pagination;

public class ProdutosFiltroPreco : QueryStringParameters
{
    public  decimal? Preco { get; set; }
    public string? PrecoCriterio { get; set; } //maior, menor ou igual

}
