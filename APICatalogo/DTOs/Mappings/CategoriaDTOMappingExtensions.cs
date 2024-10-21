using APICatalogo.Models;

namespace APICatalogo.DTOs.Mappings;

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

        return categorias.Select(c => new CategoriaDTO() {
            CategoriaId = c.CategoriaId,
            Nome = c.Nome,
            ImagemUrl = c.ImagemUrl,
        } as CategoriaDTO).ToList();

    }
}
