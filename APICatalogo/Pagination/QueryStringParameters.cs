namespace APICatalogo.Pagination;
/*
    - Classe Abstratsa não pode ser instanciada
    - Pode ter propriedades e métodos que servirão de base para outras classes. 
    - Serve para reutilizar o código
 */
public abstract class QueryStringParameters
{
    const int maxPageSize = 50;

    public int PageNumber { get; set; } = 1;
    private int _pageSize = maxPageSize;

    public int PageSize
    {
        get { return _pageSize; }

        set
        {   //value -> valor passado para a propriedade
            _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
