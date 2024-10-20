namespace APICatalogo.Repositories;

public interface IUnitOfWork
{
    //Agrupa as operações relacionadas ao repositório
    //Persiste os dados no BD

    IProdutoRepository ProdutoRepository { get; }
    ICategoriaRepository CategoriaRepository { get; }

    //Confirma as alterações pendentes no Repository
    //Chama o método SaveChanges()
    void Commit();
}
