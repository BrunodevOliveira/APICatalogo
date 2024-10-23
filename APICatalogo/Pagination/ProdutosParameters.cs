namespace APICatalogo.Pagination;

//Parâmetros que serão passados ao Request
public class ProdutosParameters
{
    const int maxPageSize = 50;

    public int PageNumber { get; set; } = 1;
    private int _pageSize;

    public int PageSize 
    { 
        get {  return _pageSize; }
        
        set
        {   //value -> valor passado para a propriedade
            _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
