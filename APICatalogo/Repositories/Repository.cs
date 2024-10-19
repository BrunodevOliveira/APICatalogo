using APICatalogo.Context;
using System.Linq.Expressions;

namespace APICatalogo.Repositories;

/*
    - Repository é uma classe genérica que implementa uma interface Genérica
    - where T : class -> é uyma restrição para garantir que o Tipo T seja uma classe
 */
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext contex)
    {
        _context = contex;
    }

    public IEnumerable<T> GetAll()
    {
        //Método Set é utilizado para acessar uma tabela ou coleção
        return _context.Set<T>().ToList();
    }

    public T? Get(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().FirstOrDefault(predicate);
    }

    public T Create(T entity)
    {
        _context.Set<T>().Add(entity);
        _context.SaveChanges();
        return entity; 
    }

    public T Update(T entity)
    {
        _context.Set<T>().Update(entity);
        _context.SaveChanges(); 

        return entity;
    }

    public T Delete(T entity)
    {
        _context.Set<T>().Remove(entity);

        _context.SaveChanges();

        return entity;
    }
}
