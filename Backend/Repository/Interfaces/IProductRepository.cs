using Backend.DTOs;
using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYER 1: PRODUCT REPOSITORY INTERFACE
    // ════════════════════════════════════════════════════════════════════════
    // Defines WHAT database operations exist for products.
    // ProductRepository.cs implements HOW they work using EF Core + Oracle.
    //
    // REGISTER in Program.cs:
    //   builder.Services.AddScoped<IProductRepository, ProductRepository>();
    // ════════════════════════════════════════════════════════════════════════

    public interface IProductRepository
    {
        // ── Read ─────────────────────────────────────────────────────────

        /// <summary>
        /// Get one product by its ID.
        /// Includes: Seller, Category, Reviews (for detail page).
        /// Returns null if not found.
        /// </summary>
        Task<Product?> GetByIdAsync(int id);

        /// <summary>
        /// Get one product by its slug (URL-friendly title).
        /// E.g. GetBySlugAsync("nike-air-max-90")
        /// </summary>
        Task<Product?> GetBySlugAsync(string slug);

        /// <summary>
        /// Get filtered, sorted, paginated list of products.
        /// Called by the browse/search page.
        /// Returns (list of products, total count before pagination).
        /// </summary>
        Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(ProductFilterDto filter);

        /// <summary>
        /// Get all products belonging to a specific seller.
        /// Used by the Seller Dashboard.
        /// </summary>
        Task<IEnumerable<Product>> GetBySellerIdAsync(int sellerId);

        /// <summary>
        /// Check if a product with the given slug already exists.
        /// Used to ensure slugs are unique before saving.
        /// </summary>
        Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null);

        // ── Write ────────────────────────────────────────────────────────

        /// <summary>Save a new product to Oracle.</summary>
        Task<Product> AddAsync(Product product);

        /// <summary>Update an existing product in Oracle.</summary>
        Task<Product> UpdateAsync(Product product);

        /// <summary>
        /// Delete a product by ID.
        /// Only the owning Seller or an Admin should call this.
        /// </summary>
        Task DeleteAsync(int id);
    }
}