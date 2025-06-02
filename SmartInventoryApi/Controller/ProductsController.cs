using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryApi.DTOs;
using SmartInventoryApi.Services;
using System.Security.Claims; // For ClaimTypes

namespace SmartInventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu xác thực cho tất cả
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        // Cho phép Admin, Manager, Staff xem danh sách sản phẩm
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductQueryParameters queryParameters)
        {
            var paginatedProducts = await _productService.GetAllProductsAsync(queryParameters);
            return Ok(paginatedProducts);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin và Manager được tạo sản phẩm
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            try
            {
                var newProduct = await _productService.CreateProductAsync(createProductDto, userId);
                return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
            }
            catch (InvalidOperationException ex) // Ví dụ: code/name đã tồn tại
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex) // Ví dụ: CategoryId không tìm thấy
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                // Hoặc trả về Forbid() nếu muốn rõ ràng hơn về quyền
                return Unauthorized("User ID claim not found or invalid for update operation.");
            }

            try
            {
                await _productService.UpdateProductAsync(id, updateProductDto, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Admin và Manager có thể xóa (soft delete)
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID claim not found or invalid for delete operation.");
            }

            try
            {
                await _productService.DeleteProductAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}