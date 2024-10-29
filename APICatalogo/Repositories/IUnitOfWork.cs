namespace APICatalogo.Repositories;

/*
     "Unit of Work" pattern:
        - As operações de escrita apenas preparam as mudanças
        - O SaveChanges é que realmente persiste tudo no banco
        - Apenas as operações que realmente acessam o banco são assíncronas
 */
public interface IUnitOfWork
{
    //Agrupa as operações relacionadas ao repositório
    //Persiste os dados no BD

    IProdutoRepository ProdutoRepository { get; }
    ICategoriaRepository CategoriaRepository { get; }

    //Confirma as alterações pendentes no Repository
    //Chama o método SaveChanges()
    Task CommitAsync();
}
