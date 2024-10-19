using APICatalogo.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APICatalogo.Models;

//Classe Anêmica -> não possui métodos apenas propriedades
public class Produto : IValidatableObject //Serve apenas para implementar um  método com validações personalizadas
{
    [Key]
    public int ProdutoId { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(80)]
    [PrimeiraLetraMaiuscula] //Validação é executada antes de entrar no Controller! 
    public string? Nome { get; set; }

    [Required]
    [StringLength(300)]
    public string? Descricao { get; set; }

    [Required]
    [Column(TypeName ="decimal(10,2)")]
    public decimal Preco { get; set; }

    [Required]
    [StringLength(300)]
    public string? ImagemUrl { get; set; }

    public float Estoque { get; set; }

    public DateTime DataCadastro { get; set; }

    /*
        Adicionando relacionamento:
        - Incluo uma propriedade CategoriaId que mapeia para a chave estrangeira no BD
        - Incluo uma propriedade Categoria (prop de navegação) para indicar que um Produto esta relacionado com uma Categoria.
     */
    public int CategoriaId { get; set; }

    [JsonIgnore]
    public Categoria? Categoria { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(!string.IsNullOrEmpty(this.Nome))
        {
            var primeiraLetra = this.Nome[0].ToString();
            if(primeiraLetra != primeiraLetra.ToUpper())
            {
                yield return new ValidationResult("A primeira letra do nome do produto deve ser maiúscula",
                    new[] { nameof(this.Nome) }
                );
            }
        }

        if(this.Estoque <= 0)
        {
            yield return new ValidationResult("O estoque deve ser maior que 0",
                   new[] { nameof(this.Nome) }
            );
        }
    }
}
