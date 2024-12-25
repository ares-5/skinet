using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public sealed class ProductsController(IUnitOfWork unit) : BaseApiController
{
    private readonly IUnitOfWork unit = unit;
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProductsAsync(
        [FromQuery] ProductSpecParams specParams)
    {
        var spec = new ProductSpecification(specParams);
        
        return await CreatePagedResult(
            repo: unit.Repository<Product>(),
            spec: spec,
            pageIndex: specParams.PageIndex,
            pageSize: specParams.PageSize);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProductAsync(int id)
    {
        var product = await unit.Repository<Product>().GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProductAsync(Product product)
    {
        unit.Repository<Product>().Add(product);
        if (await unit.CompleteAsync())
        {
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        return BadRequest("Problem creating product");
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProductAsync(int id, Product product)
    {
        if (product.Id != id || !await unit.Repository<Product>().ExistsAsync(id))
        {
            return BadRequest("Cannot update this product");
        }
        
        unit.Repository<Product>().Update(product);
        if (await unit.CompleteAsync())
        {
            return NoContent();
        }
        
        return BadRequest("Problem updating product");
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Product>> DeleteProductAsync(int id)
    {
        var product = await unit.Repository<Product>().GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }
        
        unit.Repository<Product>().Remove(product);
        if (await unit.CompleteAsync())
        {
            return NoContent();
        }
        
        return BadRequest("Problem deleting product");
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrandsAsync()
    {
        var spec = new BrandListSpecification();
        return Ok(await unit.Repository<Product>().ListAsync(spec));
    }
    
    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypesAsync()
    {
        var spec = new TypeListSpecification();
        return Ok(await unit.Repository<Product>().ListAsync(spec));
    }
}