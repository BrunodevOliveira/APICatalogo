using APICatalogo.Controllers;
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ApiCatalogoxUnitTests.UnitTests.ProdutoController;

//IClassFixture -> Permite instanciar ProdutosUnitTestController apenas uma vez e compartilhar essa instancia com todos os testes
public class GetProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
{
    private readonly ProdutosController _controller;

    public GetProdutoUnitTests(ProdutosUnitTestController controller)
    {
        _controller = new ProdutosController(controller.repository, controller.mapper);
    }

    [Fact]
    public async Task GetProdutoById_OkResult()
    {
        //Arrange
        var prodId = 2;
        
        //Act
        var data = await _controller.Get(prodId);
        
        //Assert (xunit)
        // var okResult = Assert.IsType<OkObjectResult>(data);
        // Assert.Equal(200, okResult.StatusCode);
        
        //Assert (Fluentassertions)
        //Verifica se o resultado é do tipo OkObjectResult é 200
         data.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetProdutoById_NotFound()
    {
        //Arrange
        var prodId = 999;
        
        // Act
        var data = await _controller.Get(prodId);
        
        //Assert
        data.Result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
        
    }
    
    [Fact]
    public async Task GetProdutoById_BadRequest()
    {
        //Arrange
        var prodId = -1;
        
        // Act
        var data = await _controller.Get(prodId);
        
        //Assert
        data.Result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
        
    }

    [Fact]
    public async Task GetProdutos_ListOfProdutoDTO()
    {
        //Act
        var data = await _controller.Get();
        
        //Assert
        data.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeAssignableTo<IEnumerable<ProdutoDTO>>()
            .And.NotBeNull();
    }

    [Fact]
    public async Task GetProdutos_BadRequest()
    {
        var data = await _controller.Get();
        data.Result.Should().BeOfType<BadRequestResult>().Which.StatusCode.Should().Be(400);
    }
}