using Backend.DTOs;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Backend.Services
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYER 2: PRODUCT SERVICE (Implementation)
    // ════════════════════════════════════════════════════════════════════════
    // All product business logic lives here.
    //
    // RESPONSIBILITIES:
    //   ✅ Generate URL-friendly slugs from titles
    //   ✅ Upload images to Cloudinary, get back URL
    //   ✅ Delete old Cloudinary images when product is updated/deleted
    //   ✅ Enforce seller ownership (sellers can only edit their own products)
    //   ✅ Map Product entity → ProductResponseDto (never expose raw model)
    //   ✅ Handle pagination metadata (TotalPages, HasNextPage etc.)
    //
    // DEPENDENCIES (injected by .NET):
    //   IProductRepository  → Oracle DB access
    //   CloudinaryService   → image upload/delete
    //   ILogger             → log to console
    // ════════════════════════════════════════════════════════════════════════

    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            CloudinaryService cloudinaryService,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // ════════════════════════════════════════════════════════════════════
        // GET PRODUCTS (browse / search / filter)
        // ════════════════════════════════════════════════════════════════════
        public async Task<PaginatedProductsDto> GetProductsAsync(ProductFilterDto filter)
        {
            var (products, totalCount) = await _productRepository.GetAllAsync(filter);

            var pageSize = Math.Clamp(filter.PageSize, 1, 50);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PaginatedProductsDto
            {
                Items = products.Select(MapToListItemDto),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPrevPage = filter.Page > 1
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // GET SINGLE PRODUCT BY ID
        // ════════════════════════════════════════════════════════════════════
        public async Task<ProductResponseDto> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException("Product", id);

            return MapToResponseDto(product);
        }

        // ════════════════════════════════════════════════════════════════════
        // GET SINGLE PRODUCT BY SLUG
        // ════════════════════════════════════════════════════════════════════
        public async Task<ProductResponseDto> GetProductBySlugAsync(string slug)
        {
            var product = await _productRepository.GetBySlugAsync(slug);

            if (product == null)
                throw new NotFoundException($"Product with slug '{slug}' not found");

            return MapToResponseDto(product);
        }

        // ════════════════════════════════════════════════════════════════════
        // CREATE PRODUCT
        // ════════════════════════════════════════════════════════════════════
        public async Task<ProductResponseDto> CreateProductAsync(
            CreateProductDto dto,
            int sellerId,
            IFormFile? image)
        {
            _logger.LogInformation("Seller {SellerId} creating product: {Title}", sellerId, dto.Title);

            // ── Step 1: Generate unique slug from title ──────────────────
            var slug = await GenerateUniqueSlugAsync(dto.Title);

            // ── Step 2: Upload image to Cloudinary (if provided) ─────────
            string? imageUrl = null;
            string? imagePublicId = null;

            if (image != null)
            {
                ValidateImageFile(image);
                var uploadResult = await _cloudinaryService.UploadProductImageAsync(image);
                imageUrl = uploadResult.SecureUrl;
                imagePublicId = uploadResult.PublicId;
                _logger.LogInformation("Image uploaded to Cloudinary: {PublicId}", imagePublicId);
            }

            // ── Step 3: Build the Product entity ─────────────────────────
            var product = new Product
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Slug = slug,
                Price = dto.Price,
                DiscountPercent = dto.DiscountPercent,
                Stock = dto.Stock,
                Unit = dto.Unit.ToLower().Trim(),
                CategoryId = dto.CategoryId,
                SellerId = sellerId,       // from JWT — seller cannot fake this
                ImageUrl = imageUrl,
                ImagePublicId = imagePublicId,
                IsActive = true,
                IsApproved = true,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Before saving product");

            // ── Step 4: Save to Oracle ───────────────────────────────────
            var saved = await _productRepository.AddAsync(product);
            _logger.LogInformation("Product created with ID: {ProductId}", saved.Id);

            _logger.LogInformation("Before saving product");

            // ── Step 5: Reload with relationships for the response ────────
            var full = await _productRepository.GetByIdAsync(saved.Id);
            return MapToResponseDto(full!);
        }

        // ════════════════════════════════════════════════════════════════════
        // UPDATE PRODUCT
        // ════════════════════════════════════════════════════════════════════
        public async Task<ProductResponseDto> UpdateProductAsync(
            int productId,
            UpdateProductDto dto,
            int sellerId,
            IFormFile? image)
        {
            // ── Step 1: Load the product ─────────────────────────────────
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new NotFoundException("Product", productId);

            // ── Step 2: Ownership check — sellers can only edit THEIR products
            if (product.SellerId != sellerId)
                throw new ForbiddenException("You can only edit your own products");

            // ── Step 3: Regenerate slug if title changed ─────────────────
            if (!product.Title.Equals(dto.Title.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                product.Slug = await GenerateUniqueSlugAsync(dto.Title, productId);
            }

            // ── Step 4: Handle image changes ─────────────────────────────
            if (image != null)
            {
                // New image uploaded → delete old Cloudinary image first
                ValidateImageFile(image);

                if (!string.IsNullOrEmpty(product.ImagePublicId))
                    await _cloudinaryService.DeleteImageAsync(product.ImagePublicId);

                var uploadResult = await _cloudinaryService.UploadProductImageAsync(image);
                product.ImageUrl = uploadResult.SecureUrl;
                product.ImagePublicId = uploadResult.PublicId;
            }
            else if (dto.RemoveImage && !string.IsNullOrEmpty(product.ImagePublicId))
            {
                // Seller wants to remove image without replacing it
                await _cloudinaryService.DeleteImageAsync(product.ImagePublicId);
                product.ImageUrl = null;
                product.ImagePublicId = null;
            }
            // else: no new image + RemoveImage=false → keep existing image

            // ── Step 5: Update fields ─────────────────────────────────────
            product.Title = dto.Title.Trim();
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.DiscountPercent = dto.DiscountPercent;
            product.Stock = dto.Stock;
            product.Unit = dto.Unit.ToLower().Trim();
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            // ── Step 6: Save ──────────────────────────────────────────────
            await _productRepository.UpdateAsync(product);
            _logger.LogInformation("Product {ProductId} updated by seller {SellerId}", productId, sellerId);

            var full = await _productRepository.GetByIdAsync(productId);
            return MapToResponseDto(full!);
        }

        // ════════════════════════════════════════════════════════════════════
        // TOGGLE ACTIVE (Seller hides/shows their listing)
        // ════════════════════════════════════════════════════════════════════
        public async Task<ProductResponseDto> ToggleActiveAsync(int productId, int sellerId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new NotFoundException("Product", productId);

            if (product.SellerId != sellerId)
                throw new ForbiddenException("You can only manage your own products");

            product.IsActive = !product.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            _logger.LogInformation(
                "Product {ProductId} IsActive toggled to {IsActive}", productId, product.IsActive);

            return MapToResponseDto(product);
        }

        // ════════════════════════════════════════════════════════════════════
        // DELETE PRODUCT
        // ════════════════════════════════════════════════════════════════════
        public async Task DeleteProductAsync(
            int productId,
            int requestingUserId,
            string requestingUserRole)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new NotFoundException("Product", productId);

            // Sellers can only delete their own products
            // Admins can delete any product
            var isAdmin = requestingUserRole == UserRoles.Admin;
            var isOwner = product.SellerId == requestingUserId;

            if (!isAdmin && !isOwner)
                throw new ForbiddenException("You can only delete your own products");

            // Delete image from Cloudinary first
            if (!string.IsNullOrEmpty(product.ImagePublicId))
            {
                await _cloudinaryService.DeleteImageAsync(product.ImagePublicId);
                _logger.LogInformation("Deleted Cloudinary image: {PublicId}", product.ImagePublicId);
            }

            await _productRepository.DeleteAsync(productId);
            _logger.LogInformation("Product {ProductId} deleted by user {UserId}", productId, requestingUserId);
        }

        // ════════════════════════════════════════════════════════════════════
        // ADMIN: SET APPROVAL
        // ════════════════════════════════════════════════════════════════════
        public async Task<ProductResponseDto> SetApprovalAsync(int productId, bool isApproved)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new NotFoundException("Product", productId);

            product.IsApproved = isApproved;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            _logger.LogInformation("Product {ProductId} approval set to {IsApproved}", productId, isApproved);

            return MapToResponseDto(product);
        }

        // ════════════════════════════════════════════════════════════════════
        // ADMIN: SET FEATURED
        // ════════════════════════════════════════════════════════════════════
        public async Task<ProductResponseDto> SetFeaturedAsync(int productId, bool isFeatured)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new NotFoundException("Product", productId);

            product.IsFeatured = isFeatured;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            _logger.LogInformation("Product {ProductId} featured set to {IsFeatured}", productId, isFeatured);

            return MapToResponseDto(product);
        }

        // ════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════

        // ── Generate a unique URL slug from a title ───────────────────────
        // "Nike Air Max 90!" → "nike-air-max-90"
        // If "nike-air-max-90" already exists → "nike-air-max-90-2"
        private async Task<string> GenerateUniqueSlugAsync(string title, int? excludeId = null)
        {
            // 1. Lowercase
            var slug = title.ToLower().Trim();

            // 2. Replace spaces and underscores with hyphens
            slug = Regex.Replace(slug, @"[\s_]+", "-");

            // 3. Remove all characters that are not letters, digits, or hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // 4. Collapse multiple consecutive hyphens
            slug = Regex.Replace(slug, @"-{2,}", "-");

            // 5. Trim leading/trailing hyphens
            slug = slug.Trim('-');

            // 6. Truncate to 200 chars
            if (slug.Length > 200)
                slug = slug[..200].TrimEnd('-');

            // 7. Ensure uniqueness — add suffix if slug already exists
            var baseSlug = slug;
            var counter = 2;

            while (await _productRepository.SlugExistsAsync(slug, excludeId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        // ── Validate uploaded image file ──────────────────────────────────
        private static void ValidateImageFile(IFormFile file)
        {
            // Max 5 MB
            const long maxSizeBytes = 5 * 1024 * 1024;
            if (file.Length > maxSizeBytes)
                throw new ValidationException("Image file size cannot exceed 5 MB");

            // Only allow image MIME types
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ValidationException("Only JPEG, PNG, and WebP images are allowed");
        }

        // ── Map Product entity → full ProductResponseDto ──────────────────
        private static ProductResponseDto MapToResponseDto(Product p)
        {
            return new ProductResponseDto
            {
                Id = p.Id,
                Slug = p.Slug,
                Title = p.Title,
                Description = p.Description,
                Unit = p.Unit,
                Price = p.Price,
                DiscountPercent = p.DiscountPercent,
                FinalPrice = p.FinalPrice,
                Stock = p.Stock,
                InStock = p.InStock,
                HasDiscount = p.HasDiscount,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                IsApproved = p.IsApproved,
                IsFeatured = p.IsFeatured,
                SellerId = p.SellerId,
                SellerName = p.Seller?.Name ?? "Unknown",
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }

        // ── Map Product entity → lightweight ProductListItemDto ───────────
        private static ProductListItemDto MapToListItemDto(Product p)
        {
            // Truncate description to 150 chars for list view
            var shortDesc = p.Description.Length > 150
                ? p.Description[..150].TrimEnd() + "..."
                : p.Description;

            return new ProductListItemDto
            {
                Id = p.Id,
                Slug = p.Slug,
                Title = p.Title,
                ShortDesc = shortDesc,
                Price = p.Price,
                DiscountPercent = p.DiscountPercent,
                FinalPrice = p.FinalPrice,
                Stock = p.Stock,
                InStock = p.InStock,
                HasDiscount = p.HasDiscount,
                ImageUrl = p.ImageUrl,
                SellerName = p.Seller?.Name ?? "Unknown",
                CategoryName = p.Category?.Name,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                IsFeatured = p.IsFeatured,
                CreatedAt = p.CreatedAt
            };
        }
    }
}