using PharmacySystem.Models.Entities;
using PharmacySystem.Models.ViewModels;

namespace PharmacySystem.Services.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync(string? search = null, int? categoryId = null);
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByBarcodeAsync(string barcode);
    Task<List<ProductAutocompleteItem>> SearchAutocompleteAsync(string term);
    Task<Product> CreateAsync(ProductFormVM model);
    Task UpdateAsync(int id, ProductFormVM model);
    Task DeleteAsync(int id);
}
