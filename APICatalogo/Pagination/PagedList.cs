namespace APICatalogo.Pagination
{
    public class PagedList <T> : List<T> where T : class //Herda de List<T>
    {
        public int CurrentPage { get; set; } //representa a página atual
        public int TotalPages { get; set; } //número total de páginas
        public int PageSize { get; set; } // Tamanho da página
        public int TotalCount { get; set; } // Total de itens

        public bool HasPrevious => CurrentPage > 1; //indica se existe próxima página

        public bool HasNext => CurrentPage < TotalPages; //Indica se existe próxima página


        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(items);
        }

        public static PagedList<T> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();

            /*
                Para a página 1, PageNumber é 1. Então (1 - 1) * 10 é 0, logo Skip(0) não pula nenhum item e Take(10) pega os primeiros 10 itens.

                Para a página 2, PageNumber é 2. Então (2 - 1) * 10 é 10, logo Skip(10) pula os primeiros 10 itens e Take(10) pega os próximos 10 itens.

                E assim por diante.
            */
            var items = source
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)//Pula um número específico de elementos
                    .ToList(); // Retorna o número de elementos que serão exibidos

            return new PagedList<T>(items, count, pageNumber, pageSize); //Retorno uma instância de PagedList
        }
    }
}
 