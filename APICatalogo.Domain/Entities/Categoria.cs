using System.Collections.Generic; // For ICollection
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APICatalogo.Domain.Entities;

[Table("Categorias")]
public class Categoria
{
    public Categoria()
    {
        //Boa pratica inicializar a coleção
        Produtos = new Collection<Produto>();
    }

    [Key]
    public int CategoriaId { get; set; }

    [Required]
    [StringLength(80)]
    public string? Nome { get; set; }
      
    [Required]
    [StringLength(300)]
    public string? ImagemUrl { get; set; }

    //Propriedade de navegação (ICollection) que define que uma Categoria pode conter uma coleção de produtos
    [JsonIgnore]
    public ICollection<Produto>? Produtos { get; set; }
}
