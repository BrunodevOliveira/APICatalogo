using APICatalogo.Controllers;
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ApiCatalogoxUnitTests.UnitTests.ProdutoController;

public class PostProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
{
    private readonly ProdutosController _controller;

    public PostProdutoUnitTests(ProdutosUnitTestController controller)
    {
        _controller = new ProdutosController(controller.repository, controller.mapper);
    }

    [Fact]
    public async Task PostProduto_Return_CreatedStatusCode()
    {
        //Arrange
        var novoProdutoDto = new ProdutoDTO
        {
            Nome = "Nome do produto",
            Descricao = "Descricao do produto",
            Preco = 100.00M,
            ImagemUrl = "imgfake.jpg",
            CategoriaId = 2
        };
        
        // Act
        var data = await _controller.Post(novoProdutoDto);
        
        //Assert
        var createdResult = data.Result.Should()
            .BeOfType<CreatedAtRouteResult>(); //Verifica o tipo do resultado
        
        createdResult.Subject.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task PostProduto_Return_BadRequest()
    {
        //Arrange
        ProdutoDTO novoProdutoDto = null;
        
        //Act
        var data = await _controller.Post(novoProdutoDto);
        
        //Assert
        var createdResult = data.Result.Should()
            .BeOfType<BadRequestObjectResult>();
        
        createdResult.Subject.StatusCode.Should().Be(400);
    }
}