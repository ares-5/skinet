using API.RequestHelpers;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public sealed class ProductsController(IRepository<Product> repository) : BaseApiController
{
    private readonly IRepository<Product> repository = repository;
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProductsAsync(
        [FromQuery] ProductSpecParams specParams)
    {
        var spec = new ProductSpecification(specParams);
        
        return await CreatePagedResult(
            repo: repository,
            spec: spec,
            pageIndex: specParams.PageIndex,
            pageSize: specParams.PageSize);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProductAsync(int id)
    {
        var product = await repository.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProductAsync(Product product)
    {
        repository.Add(product);
        if (await repository.SaveAllAsync())
        {
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        return BadRequest("Problem creating product");
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProductAsync(int id, Product product)
    {
        if (product.Id != id || !await repository.ExistsAsync(id))
        {
            return BadRequest("Cannot update this product");
        }
        
        repository.Update(product);
        if (await repository.SaveAllAsync())
        {
            return NoContent();
        }
        
        return BadRequest("Problem updating product");
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Product>> DeleteProductAsync(int id)
    {
        var product = await repository.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }
        
        repository.Remove(product);
        if (await repository.SaveAllAsync())
        {
            return NoContent();
        }
        
        return BadRequest("Problem deleting product");
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrandsAsync()
    {
        var spec = new BrandListSpecification();
        return Ok(await repository.ListAsync(spec));
    }
    
    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypesAsync()
    {
        var spec = new TypeListSpecification();
        return Ok(await repository.ListAsync(spec));
    }
}