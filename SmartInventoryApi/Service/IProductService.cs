using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IProductService
    {
        Task<PaginatedResponseDto<ProductDto>> GetAllProductsAsync(ProductQueryParameters queryParameters);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto productDto, int createdByUserId);
        Task UpdateProductAsync(int id, UpdateProductDto productDto, int updatedByUserId);
        Task DeleteProductAsync(int id, int deletedByUserId);
    }
}