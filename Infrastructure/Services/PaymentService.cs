using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;
using Product = Core.Entities.Product;

namespace Infrastructure.Services;

public sealed class PaymentService(
    IConfiguration config,
    ICartService cartService,
    IUnitOfWork unit) : IPaymentService
{
    private readonly IConfiguration config = config;
    private readonly ICartService cartService = cartService;
    private readonly IUnitOfWork unit = unit;

    public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {
        var shippingPrice = 0m;
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];
        
        var cart = await cartService.GetCartAsync(cartId);
        if (cart is null)
        {
            return null;
        }

        if (cart.DeliveryMethodId.HasValue)
        {
            var deliveryMethod = await unit.Repository<DeliveryMethod>().GetByIdAsync(cart.DeliveryMethodId.Value);
            if (deliveryMethod is null)
            {
                return null;
            }
            
            shippingPrice = deliveryMethod.Price;
        }

        foreach (var item in cart.Items)
        {
            var productItem = await unit.Repository<Product>().GetByIdAsync(item.ProductId);
            if (productItem is null)
            {
                return null;
            }

            if (item.Price != productItem.Price)
            {
                item.Price = productItem.Price;
            }
        }

        var service = new PaymentIntentService();
        PaymentIntent? intent;

        if (string.IsNullOrWhiteSpace(cart.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100)) +  (long)shippingPrice * 100,
                Currency = "usd",
                PaymentMethodTypes = ["card"]
            };
            intent = await service.CreateAsync(options);
            cart.PaymentIntentId = intent.Id;
            cart.ClientSecret = intent.ClientSecret;
        }
        else
        {
            var options = new PaymentIntentUpdateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100)) + (long)shippingPrice * 100
            };
            intent = await service.UpdateAsync(cart.PaymentIntentId, options);
        }

        await cartService.SetCartAsync(cart);
        return cart;
    }
}