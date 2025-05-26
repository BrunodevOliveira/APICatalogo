using APICatalogo.Application.Validation; // Will be APICatalogo.Application.Validation
// using APICatalogo.Domain.Entities; // If it were to use domain entities directly
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace APICatalogo.Application.DTOs;

public class ProdutoDTO
{
    public int ProdutoId { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(80)]
    [PrimeiraLetraMaiuscula] //Validação é executada antes de entrar no Controller! 
    public string? Nome { get; set; }

    [Required]
    [StringLength(300)]
    public string? Descricao { get; set; }

    [Required]
    public decimal Preco { get; set; }

    [Required]
    [StringLength(300)]
    public string? ImagemUrl { get; set; }

    public int CategoriaId { get; set; }
}
