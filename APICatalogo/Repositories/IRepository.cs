using System.Linq.Expressions;

namespace APICatalogo.Repositories;

//T representa o tipo do repositório(Classe) a ser implementado
//Essa interface é herdada pelos repositórios específicos
public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();

    //Expressio -> Representa uma função Lambda
    //Func<T, bool> -> Delegate que representa uma função lambda que recebe um obj do tipo T e reotrna um bool 
    //predicate -> O critério que será usado para filtrar
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate);

    T Create(T entity);

    T Update(T entity);
    T Delete(T entity);
}
