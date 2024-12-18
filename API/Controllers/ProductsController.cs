using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductRepository repository) : ControllerBase
{
    private readonly IProductRepository repository = repository;
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProductsAsync(
        string? brand,
        string? type,
        string? sort)
    {
        return Ok(await repository.GetProductsAsync(brand: brand, type: type, sort: sort));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProductAsync(int id)
    {
        var product = await repository.GetProductByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProductAsync(Product product)
    {
        repository.AddProduct(product);
        if (await repository.SaveChangesAsync())
        {
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        return BadRequest("Problem creating product");
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProductAsync(int id, Product product)
    {
        if (product.Id != id || !await repository.ProductExistsAsync(id))
        {
            return BadRequest("Cannot update this product");
        }
        
        repository.UpdateProduct(product);
        if (await repository.SaveChangesAsync())
        {
            return NoContent();
        }
        
        return BadRequest("Problem updating product");
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Product>> DeleteProductAsync(int id)
    {
        var product = await repository.GetProductByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }
        
        repository.DeleteProduct(product);
        if (await repository.SaveChangesAsync())
        {
            return NoContent();
        }
        
        return BadRequest("Problem deleting product");
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrandsAsync()
    {
        return Ok(await repository.GetBrandsAsync());
    }
    
    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypesAsync()
    {
        return Ok(await repository.GetTypesAsync());
    }
}