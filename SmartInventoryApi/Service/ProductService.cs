using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository; // Để kiểm tra CategoryId
        private readonly IPartnerRepository _partnerRepository;   // Để kiểm tra DefaultSupplierId
        private readonly IUserRepository _userRepository;         // Để lấy thông tin người tạo/sửa

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IPartnerRepository partnerRepository,
            IUserRepository userRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _partnerRepository = partnerRepository;
            _userRepository = userRepository;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto, int createdByUserId)
        {
            if (await _productRepository.CodeExistsAsync(productDto.Code))
            {
                throw new InvalidOperationException("Product code already exists.");
            }
            if (await _productRepository.NameExistsAsync(productDto.Name))
            {
                throw new InvalidOperationException("Product name already exists.");
            }

            var category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            if (productDto.DefaultSupplierId.HasValue)
            {
                var supplier = await _partnerRepository.GetByIdAsync(productDto.DefaultSupplierId.Value);
                if (supplier == null || supplier.Type != "Supplier")
                {
                    throw new KeyNotFoundException("Default supplier not found or is not a supplier.");
                }
            }

            var user = await _userRepository.GetByIdAsync(createdByUserId);
            if (user == null)
            {
                throw new KeyNotFoundException("User creating the product not found.");
            }


            var product = new Product
            {
                Code = productDto.Code,
                Barcode = productDto.Barcode,
                Name = productDto.Name,
                Description = productDto.Description,
                CategoryId = productDto.CategoryId,
                Unit = productDto.Unit,
                DefaultSupplierId = productDto.DefaultSupplierId,
                CostPrice = productDto.CostPrice,
                SellingPrice = productDto.SellingPrice,
                MinimumStock = productDto.MinimumStock,
                MaximumStock = productDto.MaximumStock,
                ImageUrl = productDto.ImageUrl,
                IsActive = true, // Mặc định sản phẩm mới là active
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdByUserId
            };

            var createdProduct = await _productRepository.CreateAsync(product);

            // Sau khi tạo, tải lại để có thông tin Category, Supplier, CreatedByNavigation
            var fullProduct = await _productRepository.GetByIdAsync(createdProduct.Id);
            return MapToDto(fullProduct!);
        }

        public async Task DeleteProductAsync(int id, int deletedByUserId)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }
            // Ghi nhận người xóa, ví dụ: product.LastModifiedBy = deletedByUserId;
            await _productRepository.DeleteAsync(product);
        }

        public async Task<PaginatedResponseDto<ProductDto>> GetAllProductsAsync(ProductQueryParameters queryParameters)
        {
            var products = await _productRepository.GetAllAsync(queryParameters);
            var totalCount = await _productRepository.GetTotalCountAsync(queryParameters);

            var productDtos = products.Select(p => MapToDto(p));

            return new PaginatedResponseDto<ProductDto>(productDtos, queryParameters.PageNumber, queryParameters.PageSize, totalCount);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? null : MapToDto(product);
        }

        public async Task UpdateProductAsync(int id, UpdateProductDto productDto, int updatedByUserId)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            // Code không cho phép thay đổi, chỉ kiểm tra Name
            if (await _productRepository.NameExistsAsync(productDto.Name, id))
            {
                throw new InvalidOperationException("Product name already exists.");
            }

            var category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            if (productDto.DefaultSupplierId.HasValue)
            {
                var supplier = await _partnerRepository.GetByIdAsync(productDto.DefaultSupplierId.Value);
                if (supplier == null || supplier.Type != "Supplier")
                {
                    throw new KeyNotFoundException("Default supplier not found or is not a supplier.");
                }
            }

            product.Barcode = productDto.Barcode;
            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.CategoryId = productDto.CategoryId;
            product.Unit = productDto.Unit;
            product.DefaultSupplierId = productDto.DefaultSupplierId;
            product.CostPrice = productDto.CostPrice;
            product.SellingPrice = productDto.SellingPrice;
            product.MinimumStock = productDto.MinimumStock;
            product.MaximumStock = productDto.MaximumStock;
            product.ImageUrl = productDto.ImageUrl;
            product.IsActive = productDto.IsActive;

            await _productRepository.UpdateAsync(product);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Code = product.Code,
                Barcode = product.Barcode,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Unit = product.Unit,
                DefaultSupplierId = product.DefaultSupplierId,
                DefaultSupplierName = product.DefaultSupplier?.Name,
                CostPrice = product.CostPrice,
                SellingPrice = product.SellingPrice,
                MinimumStock = product.MinimumStock,
                MaximumStock = product.MaximumStock,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                CreatedDate = product.CreatedDate,
                CreatedBy = product.CreatedBy,
                CreatedByFullName = product.CreatedByNavigation?.FullName
            };
        }
    }
}