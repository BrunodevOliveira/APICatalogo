using APICatalogo.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ApiCatalogoxUnitTests.UnitTests.ProdutoController;

public class DeleteProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
{
    private readonly ProdutosController _controller;

    public DeleteProdutoUnitTests(ProdutosUnitTestController controller)
    {
        _controller = new ProdutosController(controller.repository, controller.mapper);
    }
    
    
    [Fact]
    public async Task DeleteProduto_ReturnOkObjectResult()
    {
        //Arrange
        var id = 1;
        
        //Act
        var data = await _controller.Delete(id);
        
        //Assert
        data.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }
    
    [Fact]
    public async Task DeleteProduto_ReturnNotFound()
    {
        //Arrange
        var id = 99999;
        
        //Act
        var data = await _controller.Delete(id);
        
        //Assert
        data.Result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
}