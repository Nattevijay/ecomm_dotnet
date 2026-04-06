using Backend.DTOs;
using Backend.Helpers;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYER 3: PRODUCT CONTROLLER
    // ════════════════════════════════════════════════════════════════════════
    // Handles all HTTP requests for products.
    // Thin layer — just reads request, calls service, returns response.
    //
    // ENDPOINTS:
    //   GET    /api/products                    → browse (public)
    //   GET    /api/products/{id}               → detail by ID (public)
    //   GET    /api/products/slug/{slug}        → detail by slug (public)
    //   GET    /api/products/my                 → seller's own products
    //   POST   /api/products                    → create (Seller/Admin)
    //   PUT    /api/products/{id}               → update (Seller/Admin)
    //   PATCH  /api/products/{id}/toggle-active → show/hide (Seller/Admin)
    //   DELETE /api/products/{id}               → delete (Seller/Admin)
    //   PATCH  /api/products/{id}/approve       → Admin only
    //   PATCH  /api/products/{id}/feature       → Admin only
    // ════════════════════════════════════════════════════════════════════════

    [ApiController]
    [Route("api/[controller]")]  // → /api/products
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly JwtService      _jwtService;

        public ProductController(IProductService productService, JwtService jwtService)
        {
            _productService = productService;
            _jwtService     = jwtService;
        }

        // ════════════════════════════════════════════════════════════════════
        // GET /api/products
        // Browse, search, filter, paginate — open to everyone
        // ════════════════════════════════════════════════════════════════════
        /// <summary>
        /// Get paginated list of products with optional filters.
        /// Query params: search, categoryId, minPrice, maxPrice, sortBy, page, pageSize
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedProductsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
        {
            // [FromQuery] reads URL params: /api/products?search=nike&page=2
            var result = await _productService.GetProductsAsync(filter);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // GET /api/products/{id}
        // Full product detail by numeric ID
        // ════════════════════════════════════════════════════════════════════
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // GET /api/products/slug/{slug}
        // Full product detail by URL slug (nicer URLs for React Router)
        // ════════════════════════════════════════════════════════════════════
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductBySlug(string slug)
        {
            var result = await _productService.GetProductBySlugAsync(slug);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // GET /api/products/my
        // Seller's own products (includes inactive ones)
        // ════════════════════════════════════════════════════════════════════
        [HttpGet("my")]
        [Authorize(Roles = $"{UserRoles.Seller},{UserRoles.Admin}")]
        [ProducesResponseType(typeof(PaginatedProductsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProducts([FromQuery] ProductFilterDto filter)
        {
            // Force filter by current seller's ID (from JWT)
            // Seller cannot spoof another seller's ID
            filter.SellerId = _jwtService.GetUserIdFromToken(User);

            var result = await _productService.GetProductsAsync(filter);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // POST /api/products
        // Create new product — Seller or Admin only
        // Accepts multipart/form-data because of image file upload
        // ════════════════════════════════════════════════════════════════════
        /// <summary>
        /// Create a new product. Send as multipart/form-data.
        /// Include JSON fields + optionally an "image" file field.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Seller},{UserRoles.Admin}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateProduct(
            [FromForm] CreateProductDto dto,
            IFormFile? image)              // optional image file from multipart form
        {
            var sellerId = _jwtService.GetUserIdFromToken(User);
            var result   = await _productService.CreateProductAsync(dto, sellerId, image);

            // 201 Created with Location header pointing to the new resource
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = result.Id },
                result);
        }

        // ════════════════════════════════════════════════════════════════════
        // PUT /api/products/{id}
        // Update existing product — only the owning Seller (or Admin)
        // ════════════════════════════════════════════════════════════════════
        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{UserRoles.Seller},{UserRoles.Admin}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(
            int id,
            [FromForm] UpdateProductDto dto,
            IFormFile? image)
        {
            var sellerId = _jwtService.GetUserIdFromToken(User);
            var result   = await _productService.UpdateProductAsync(id, dto, sellerId, image);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // PATCH /api/products/{id}/toggle-active
        // Seller shows/hides their product without deleting it
        // ════════════════════════════════════════════════════════════════════
        [HttpPatch("{id:int}/toggle-active")]
        [Authorize(Roles = $"{UserRoles.Seller},{UserRoles.Admin}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var sellerId = _jwtService.GetUserIdFromToken(User);
            var result   = await _productService.ToggleActiveAsync(id, sellerId);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // DELETE /api/products/{id}
        // Sellers delete their own. Admins delete any.
        // ════════════════════════════════════════════════════════════════════
        [HttpDelete("{id:int}")]
        [Authorize(Roles = $"{UserRoles.Seller},{UserRoles.Admin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var userId = _jwtService.GetUserIdFromToken(User);
            var role   = _jwtService.GetRoleFromToken(User);

            await _productService.DeleteProductAsync(id, userId, role);
            return Ok(new { message = "Product deleted successfully" });
        }

        // ════════════════════════════════════════════════════════════════════
        // PATCH /api/products/{id}/approve  — Admin only
        // ════════════════════════════════════════════════════════════════════
        [HttpPatch("{id:int}/approve")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetApproval(int id, [FromBody] ProductAdminActionDto dto)
        {
            var result = await _productService.SetApprovalAsync(id, dto.IsApproved);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // PATCH /api/products/{id}/feature  — Admin only
        // ════════════════════════════════════════════════════════════════════
        [HttpPatch("{id:int}/feature")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetFeatured(int id, [FromBody] ProductAdminActionDto dto)
        {
            var result = await _productService.SetFeaturedAsync(id, dto.IsFeatured);
            return Ok(result);
        }
    }
}