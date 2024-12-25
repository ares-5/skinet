using API.Dtos;
using API.Extensions;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class OrdersController(
    ICartService cartService,
    IUnitOfWork unit) : BaseApiController
{
    private readonly ICartService cartService = cartService;
    private readonly IUnitOfWork unit = unit;

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrderAsync([FromBody] CreateOrderDto orderDto)
    {
        var email = User.GetEmail();
        var cart = await cartService.GetCartAsync(orderDto.CartId);
        if (cart is null)
        {
            return BadRequest("Cart not found");
        }

        if (string.IsNullOrWhiteSpace(cart.PaymentIntentId))
        {
            return BadRequest("No payment intent for this order");
        }
        
        var items = new List<OrderItem>();
        foreach (var item in cart.Items)
        {
            var productItem = await unit.Repository<Product>().GetByIdAsync(item.ProductId);
            if (productItem is null)
            {
                return BadRequest("Product not found");
            }

            var itemOrdered = new ProductItemOrdered
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                PictureUrl = item.PictureUrl
            };

            var orderItem = new OrderItem
            {
                ItemOrdered = itemOrdered,
                Price = productItem.Price,
                Quantity = item.Quantity
            };
            items.Add(orderItem);
        }
        
        var deliveryMethod = await unit.Repository<DeliveryMethod>().GetByIdAsync(orderDto.DeliveryMethodId);
        if (deliveryMethod is null)
        {
            return BadRequest("No delivery method selected");
        }

        var order = new Order
        {
            OrderItems = items,
            DeliveryMethod = deliveryMethod,
            ShippingAddress = orderDto.ShippingAddress,
            Subtotal = items.Sum(x => x.Price * x.Quantity),
            PaymentSummary = orderDto.PaymentSummary,
            PaymentIntentId = cart.PaymentIntentId,
            BuyerEmail = email
        };
        
        unit.Repository<Order>().Add(order);

        if (await unit.CompleteAsync())
        {
            return order;
        }
        
        return BadRequest("Problem creating order");
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrdersForUserAsync()
    {
        var spec = new OrderSpecification(User.GetEmail());
        var orders = await unit.Repository<Order>().ListAsync(spec);
        var ordersToReturn = orders.Select(x => x.ToDto()).ToList();
        
        return Ok(ordersToReturn);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrderByIdAsync(int id)
    {
        var spec = new OrderSpecification(User.GetEmail(), id);
        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);
        if (order is null)
        {
            return NotFound();
        }
        
        return Ok(order.ToDto());
    }
}