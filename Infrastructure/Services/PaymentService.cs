using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;
using Product = Core.Entities.Product;

namespace Infrastructure.Services;

public sealed class PaymentService(
    IConfiguration config,
    ICartService cartService,
    IRepository<Product> productRepository,
    IRepository<DeliveryMethod> dmRepository) : IPaymentService
{
    private readonly IConfiguration config = config;
    private readonly ICartService _cartService = cartService;
    private readonly IRepository<Product> _productRepository = productRepository;
    private readonly IRepository<DeliveryMethod> _dmRepository = dmRepository;

    public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {
        var shippingPrice = 0m;
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];
        
        var cart = await _cartService.GetCartAsync(cartId);
        if (cart is null)
        {
            return null;
        }

        if (cart.DeliveryMethodId.HasValue)
        {
            var deliveryMethod = await _dmRepository.GetByIdAsync(cart.DeliveryMethodId.Value);
            if (deliveryMethod is null)
            {
                return null;
            }
            
            shippingPrice = deliveryMethod.Price;
        }

        foreach (var item in cart.Items)
        {
            var productItem = await _productRepository.GetByIdAsync(item.ProductId);
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

        await _cartService.SetCartAsync(cart);
        return cart;
    }
}