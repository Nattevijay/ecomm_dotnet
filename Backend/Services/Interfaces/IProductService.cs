using Backend.DTOs;
using Microsoft.AspNetCore.Http;

namespace Backend.Services.Interfaces
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYER 2: PRODUCT SERVICE INTERFACE
    // ════════════════════════════════════════════════════════════════════════
    // Defines all product business operations.
    // ProductService.cs implements the actual logic.
    //
    // REGISTER in Program.cs:
    //   builder.Services.AddScoped<IProductService, ProductService>();
    // ════════════════════════════════════════════════════════════════════════

    public interface IProductService
    {
        // ── Public (any visitor) ──────────────────────────────────────────

        /// <summary>Browse products with filters, sorting, pagination.</summary>
        Task<PaginatedProductsDto> GetProductsAsync(ProductFilterDto filter);

        /// <summary>Get full product detail by ID.</summary>
        Task<ProductResponseDto> GetProductByIdAsync(int id);

        /// <summary>Get full product detail by URL slug.</summary>
        Task<ProductResponseDto> GetProductBySlugAsync(string slug);

        // ── Seller: own products only ─────────────────────────────────────

        /// <summary>
        /// Create a new product listing.
        /// sellerId comes from the JWT token (not from the request body).
        /// image is optional — if provided, it is uploaded to Cloudinary.
        /// </summary>
        Task<ProductResponseDto> CreateProductAsync(
            CreateProductDto dto,
            int sellerId,
            IFormFile? image);

        /// <summary>
        /// Update an existing product.
        /// Throws ForbiddenException if sellerId does not own the product.
        /// </summary>
        Task<ProductResponseDto> UpdateProductAsync(
            int productId,
            UpdateProductDto dto,
            int sellerId,
            IFormFile? image);

        /// <summary>
        /// Soft-toggle product visibility (active/inactive).
        /// Only the owning Seller can do this.
        /// </summary>
        Task<ProductResponseDto> ToggleActiveAsync(int productId, int sellerId);

        /// <summary>
        /// Permanently delete a product.
        /// Seller can only delete their own. Admin can delete any.
        /// Also deletes the Cloudinary image if one exists.
        /// </summary>
        Task DeleteProductAsync(int productId, int requestingUserId, string requestingUserRole);

        // ── Admin only ────────────────────────────────────────────────────

        /// <summary>Admin: approve or reject a product listing.</summary>
        Task<ProductResponseDto> SetApprovalAsync(int productId, bool isApproved);

        /// <summary>Admin: feature or un-feature a product on the homepage.</summary>
        Task<ProductResponseDto> SetFeaturedAsync(int productId, bool isFeatured);
    }
}