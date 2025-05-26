using APICatalogo.Application.DTOs; // For CategoriaDTO
using APICatalogo.Domain.Entities;   // For Categoria
using System.Collections.Generic;    // For IEnumerable, List
using System.Linq;                   // For Select, Any

namespace APICatalogo.Application.DTOs.Mappings;

public static class CategoriaDTOMappingExtensions
{
    public static CategoriaDTO? ToCategoriaDto(this Categoria categoria)
    {
        if (categoria is null)return null;

        var categoriaDTO = new CategoriaDTO()
        {
            CategoriaId = categoria.CategoriaId,
            Nome = categoria.Nome,
            ImagemUrl = categoria.ImagemUrl,
        };

        return categoriaDTO;
    }

    public static Categoria? ToCategoria(this CategoriaDTO categoriaDTO)
    {
        if (categoriaDTO is null) return null;

        var categoria = new Categoria()
        {
            CategoriaId = categoriaDTO.CategoriaId,
            Nome = categoriaDTO.Nome,
            ImagemUrl = categoriaDTO.ImagemUrl,
        };

        return categoria;
    }

    public static IEnumerable<CategoriaDTO>? ToCategoriasDtoList(this IEnumerable<Categoria> categorias)
    {
        if (categorias is null || !categorias.Any()) return new List<CategoriaDTO>();

        return categorias.Select(c => new CategoriaDTO() { // CategoriaDTO is already in the correct namespace
            CategoriaId = c.CategoriaId,
            Nome = c.Nome,
            ImagemUrl = c.ImagemUrl,
        }).ToList(); // Removed 'as CategoriaDTO' as it's not strictly necessary here and Select should infer type.

    }
}
