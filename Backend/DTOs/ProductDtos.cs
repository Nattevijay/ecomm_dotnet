namespace Backend.DTOs
{
    // ════════════════════════════════════════════════════════════════════════
    // PRODUCT DTOs (Data Transfer Objects)
    // ════════════════════════════════════════════════════════════════════════
    // DTOs travel over the network between React and the API.
    // They are NEVER stored in the database.
    //
    // WHY NOT JUST USE THE PRODUCT MODEL DIRECTLY?
    //   1. Security: The Product model has internal fields (SellerId, IsApproved)
    //      that clients should not be able to set directly.
    //   2. Flexibility: What comes IN (create/update) is different from what
    //      goes OUT (response with seller name, category name, rating etc.)
    //   3. Validation: DTOs have FluentValidation rules attached.
    //
    // DTOs IN THIS FILE:
    //   CreateProductDto    ← what seller sends to CREATE a product
    //   UpdateProductDto    ← what seller sends to UPDATE a product
    //   ProductResponseDto  ← what the API returns when reading a product
    //   ProductListItemDto  ← lightweight version for product grid/listings
    //   ProductFilterDto    ← query parameters for search/filter/paginate
    // ════════════════════════════════════════════════════════════════════════


    // ── DTO 1: Create Product ────────────────────────────────────────────
    /// <summary>
    /// INPUT: What a Seller sends when creating a new product.
    /// POST /api/products
    ///
    /// Sent as multipart/form-data because it includes an image file.
    /// The image file is a separate IFormFile parameter in the controller —
    /// it is NOT part of this DTO.
    ///
    /// Example JSON part:
    /// {
    ///   "title": "Nike Air Max 90",
    ///   "description": "Classic running shoe...",
    ///   "price": 8999.99,
    ///   "discountPercent": 10,
    ///   "stock": 50,
    ///   "unit": "pair",
    ///   "categoryId": 3
    /// }
    /// </summary>
    public class CreateProductDto
    {
        public string  Title           { get; set; } = string.Empty;
        public string  Description     { get; set; } = string.Empty;
        public decimal Price           { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
        public int     Stock           { get; set; } = 0;
        public string  Unit            { get; set; } = "piece";
        public int?    CategoryId      { get; set; }
    }


    // ── DTO 2: Update Product ────────────────────────────────────────────
    /// <summary>
    /// INPUT: What a Seller sends when editing their existing product.
    /// PUT /api/products/{id}
    ///
    /// Also sent as multipart/form-data.
    /// Seller can optionally upload a new image — if no image is uploaded,
    /// the existing Cloudinary image is kept.
    ///
    /// Note: Sellers CANNOT change SellerId (ownership) or IsApproved (admin-only).
    /// </summary>
    public class UpdateProductDto
    {
        public string  Title           { get; set; } = string.Empty;
        public string  Description     { get; set; } = string.Empty;
        public decimal Price           { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
        public int     Stock           { get; set; } = 0;
        public string  Unit            { get; set; } = "piece";
        public int?    CategoryId      { get; set; }

        /// <summary>
        /// Set to true if you want to remove the current image without replacing it.
        /// Default: false (keep existing image if no new image uploaded).
        /// </summary>
        public bool RemoveImage { get; set; } = false;
    }


    // ── DTO 3: Product Response (full detail) ────────────────────────────
    /// <summary>
    /// OUTPUT: Full product data returned by the API.
    /// Used for GET /api/products/{id} (detail page).
    ///
    /// Contains everything including seller info, category, ratings.
    /// This is what React renders on the ProductDetailPage.
    /// </summary>
    public class ProductResponseDto
    {
        // Core identity
        public int    Id   { get; set; }
        public string Slug { get; set; } = string.Empty;

        // Product info
        public string  Title           { get; set; } = string.Empty;
        public string  Description     { get; set; } = string.Empty;
        public string  Unit            { get; set; } = string.Empty;

        // Pricing (all three so React can show strikethrough price)
        public decimal Price           { get; set; }   // original price
        public decimal DiscountPercent { get; set; }   // e.g. 20
        public decimal FinalPrice      { get; set; }   // price after discount

        // Inventory
        public int  Stock   { get; set; }
        public bool InStock { get; set; }

        // Image
        public string? ImageUrl { get; set; }

        // Status flags
        public bool IsActive   { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
        public bool HasDiscount{ get; set; }

        // Seller info (flattened — no need for full User object)
        public int    SellerId   { get; set; }
        public string SellerName { get; set; } = string.Empty;

        // Category info (flattened)
        public int?   CategoryId   { get; set; }
        public string? CategoryName { get; set; }

        // Review aggregates
        public double AverageRating { get; set; }
        public int    ReviewCount   { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


    // ── DTO 4: Product List Item (lightweight) ────────────────────────────
    /// <summary>
    /// OUTPUT: Lightweight version for product grid/listing pages.
    /// Used for GET /api/products (list/browse page).
    ///
    /// Omits heavy fields (full description, seller details, reviews list)
    /// to keep the response fast when loading 20+ products at once.
    /// React uses this on ProductListPage for the product cards.
    /// </summary>
    public class ProductListItemDto
    {
        public int     Id             { get; set; }
        public string  Slug           { get; set; } = string.Empty;
        public string  Title          { get; set; } = string.Empty;

        // Short preview of description (first 150 chars)
        public string  ShortDesc      { get; set; } = string.Empty;

        public decimal Price          { get; set; }
        public decimal DiscountPercent{ get; set; }
        public decimal FinalPrice     { get; set; }

        public int     Stock          { get; set; }
        public bool    InStock        { get; set; }
        public bool    HasDiscount    { get; set; }

        public string? ImageUrl       { get; set; }

        public string  SellerName     { get; set; } = string.Empty;
        public string? CategoryName   { get; set; }

        public double  AverageRating  { get; set; }
        public int     ReviewCount    { get; set; }

        public bool    IsFeatured     { get; set; }
        public DateTime CreatedAt     { get; set; }
    }


    // ── DTO 5: Product Filter / Query Parameters ──────────────────────────
    /// <summary>
    /// INPUT: Query parameters for filtering, searching, and paginating products.
    /// Used with GET /api/products?search=nike&categoryId=3&minPrice=500&page=2
    ///
    /// React sends these as URL query params from the ProductListPage filters.
    /// All fields are optional — omitting them means "no filter on this field".
    /// </summary>
    public class ProductFilterDto
    {
        /// <summary>Search by keyword in title or description. E.g. ?search=nike</summary>
        public string? Search { get; set; }

        /// <summary>Filter by category. E.g. ?categoryId=3</summary>
        public int? CategoryId { get; set; }

        /// <summary>Filter by seller. E.g. ?sellerId=7 (for Seller Dashboard)</summary>
        public int? SellerId { get; set; }

        /// <summary>Minimum price filter. E.g. ?minPrice=500</summary>
        public decimal? MinPrice { get; set; }

        /// <summary>Maximum price filter. E.g. ?maxPrice=5000</summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>Only show in-stock products. E.g. ?inStockOnly=true</summary>
        public bool? InStockOnly { get; set; }

        /// <summary>Only show featured products. E.g. ?featuredOnly=true</summary>
        public bool? FeaturedOnly { get; set; }

        /// <summary>
        /// Sort order. Options:
        ///   "newest"       newest first (default)
        ///   "oldest"       oldest first
        ///   "price_asc"    cheapest first
        ///   "price_desc"   most expensive first
        ///   "rating"       highest rated first
        ///   "popular"      most reviewed first
        /// </summary>
        public string SortBy { get; set; } = "newest";

        /// <summary>Page number (1-based). Default = 1.</summary>
        public int Page { get; set; } = 1;

        /// <summary>Number of products per page. Default = 12. Max = 50.</summary>
        public int PageSize { get; set; } = 12;
    }


    // ── DTO 6: Paginated Product Response ────────────────────────────────
    /// <summary>
    /// OUTPUT: Wraps the product list with pagination metadata.
    /// React uses TotalCount and PageSize to render the pagination controls.
    ///
    /// Example response:
    /// {
    ///   "items": [...],
    ///   "totalCount": 148,
    ///   "page": 2,
    ///   "pageSize": 12,
    ///   "totalPages": 13,
    ///   "hasNextPage": true,
    ///   "hasPrevPage": true
    /// }
    /// </summary>
    public class PaginatedProductsDto
    {
        public IEnumerable<ProductListItemDto> Items      { get; set; }
            = Enumerable.Empty<ProductListItemDto>();

        public int  TotalCount  { get; set; }
        public int  Page        { get; set; }
        public int  PageSize    { get; set; }
        public int  TotalPages  { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPrevPage { get; set; }
    }


    // ── DTO 7: Admin Approve/Feature toggles ─────────────────────────────
    /// <summary>
    /// INPUT: Admin-only actions on a product.
    /// PATCH /api/admin/products/{id}/approve
    /// PATCH /api/admin/products/{id}/feature
    /// </summary>
    public class ProductAdminActionDto
    {
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
    }
}