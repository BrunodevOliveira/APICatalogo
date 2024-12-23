﻿using APICatalogo.Context;
using Microsoft.EntityFrameworkCore;
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

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        //Método Set é utilizado para acessar uma tabela ou coleção
        //AsNoTracking método que otimiza a consulta desapiliitando o gerenciamento dos objetos na memoria pelo EF
        return await _context.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(predicate);
    }

    public T Create(T entity)
    {
        _context.Set<T>().Add(entity); 
        //_context.SaveChanges();
        return entity; 
    }

    public T Update(T entity)
    {
        // Quando fazemos Update(), o EF Core:
        // 1. Verifica se a entidade tem uma chave primária definida
        // 2. Marca o estado da entidade como "Modified"
        _context.Set<T>().Update(entity);
        //_context.SaveChanges(); 

        return entity;
    }

    public T Delete(T entity)
    {
        _context.Set<T>().Remove(entity);

        //_context.SaveChanges();

        return entity;
    }
}
