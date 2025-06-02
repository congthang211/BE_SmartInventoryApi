using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto)
        {
            if (await _categoryRepository.NameExistsAsync(categoryDto.Name))
            {
                throw new InvalidOperationException("Category name already exists.");
            }

            if (categoryDto.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(categoryDto.ParentCategoryId.Value);
                if (parentCategory == null)
                {
                    throw new KeyNotFoundException("Parent category not found.");
                }
            }

            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ParentCategoryId = categoryDto.ParentCategoryId,
                IsActive = true
            };

            var createdCategory = await _categoryRepository.CreateAsync(category);
            return MapToDto(createdCategory);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }
            await _categoryRepository.DeleteAsync(category);
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category == null ? null : MapToDto(category);
        }

        public async Task UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            if (await _categoryRepository.NameExistsAsync(categoryDto.Name, id))
            {
                throw new InvalidOperationException("Category name already exists.");
            }

            if (categoryDto.ParentCategoryId.HasValue && categoryDto.ParentCategoryId.Value == id)
            {
                throw new InvalidOperationException("A category cannot be its own parent.");
            }

            if (categoryDto.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(categoryDto.ParentCategoryId.Value);
                if (parentCategory == null)
                {
                    throw new KeyNotFoundException("Parent category not found.");
                }
            }

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;
            category.ParentCategoryId = categoryDto.ParentCategoryId;
            category.IsActive = categoryDto.IsActive;

            await _categoryRepository.UpdateAsync(category);
        }

        // Phương thức private để map từ Entity sang DTO
        private static CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name
            };
        }
    }
}