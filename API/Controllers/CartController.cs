using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public sealed class CartController(ICartService cartService) : BaseApiController
{
    private readonly ICartService cartService = cartService;

    [HttpGet]
    public async Task<ActionResult<ShoppingCart>> GetCartByIdAsync(string id)
    {
        var cart = await cartService.GetCartAsync(key: id);

        return Ok(cart ?? new ShoppingCart { Id = id });
    }

    [HttpPost]
    public async Task<ActionResult<ShoppingCart>> UpdateCartAsync(ShoppingCart shoppingCart)
    {
        var updatedCart = await cartService.SetCartAsync(shoppingCart);
        if (updatedCart is null)
        {
            return BadRequest("Problem with cart");
        }

        return Ok(updatedCart);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteCartAsync(string id)
    {
        var result = await cartService.DeleteCartAsync(id);
        if (!result)
        {
            return BadRequest("Problem deleting cart");
        }
        
        return Ok();
    }
}