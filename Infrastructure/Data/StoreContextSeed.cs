using System.Text.Json;
using Core.Entities;

namespace Infrastructure.Data;

public static class StoreContextSeed
{
    public static async Task SeedAsync(StoreContext context)
    {
        if (!context.Products.Any())
        {
            var productsData = await File.ReadAllTextAsync("../Infrastructure/Data/products.json");
            var products = JsonSerializer.Deserialize<List<Product>>(productsData);

            if (products is null)
            {
                return;
            }
            
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
        
        if (!context.DeliveryMethods.Any())
        {
            var deliveryMethodsData = await File.ReadAllTextAsync("../Infrastructure/Data/delivery.json");
            var deliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(deliveryMethodsData);

            if (deliveryMethods is null)
            {
                return;
            }
            
            context.DeliveryMethods.AddRange(deliveryMethods);
            await context.SaveChangesAsync();
        }
    }
}