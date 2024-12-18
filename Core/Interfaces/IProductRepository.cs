using Core.Entities;

namespace Core.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type, string? sort);
    Task<Product?> GetProductByIdAsync(int id);
    Task<IReadOnlyList<string>> GetBrandsAsync();
    Task<IReadOnlyList<string>> GetTypesAsync();
    Task<bool> ProductExistsAsync(int id);
    void AddProduct(Product product);
    void UpdateProduct(Product product);
    void DeleteProduct(Product product);
    Task<bool> SaveChangesAsync();
}