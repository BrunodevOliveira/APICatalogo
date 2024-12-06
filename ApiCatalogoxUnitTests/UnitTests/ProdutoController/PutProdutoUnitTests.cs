using APICatalogo.Controllers;
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ApiCatalogoxUnitTests.UnitTests.ProdutoController;

public class PutProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
{
    private readonly ProdutosController _controller;

    public PutProdutoUnitTests(ProdutosUnitTestController controller)
    {
        _controller = new ProdutosController(controller.repository, controller.mapper);
    }

    [Fact]
    public async Task PutProduto_Return_OkResult()
    {
        //Arrange
        var novoProdutoDto = new ProdutoDTO
        {
            Nome = "Nome do produto",
            Descricao = "Descricao do produto",
            Preco = 100.00M,
            ImagemUrl = "imgfake.jpg",
            CategoriaId = 2, 
            ProdutoId = 2
        };

        var id = 2;
        
        //Act
        var data = await _controller.Put(id, novoProdutoDto);
        
        //Assert
        data.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }


    [Fact]
    public async Task PutProduto_Return_BadRequest()
    {
        //Arrange
        var novoProdutoDto = new ProdutoDTO
        {
            Nome = "Nome do produto",
            Descricao = "Descricao do produto",
            Preco = 100.00M,
            ImagemUrl = "imgfake.jpg",
            CategoriaId = 2, 
            ProdutoId = 2
        };

        var id = 9999;
        
        //Act
        var data = await _controller.Put(id, novoProdutoDto);
        
        //Assert
        data.Result.Should().BeOfType<BadRequestResult>().Which.StatusCode.Should().Be(400);
    }
}