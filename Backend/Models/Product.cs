using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // ════════════════════════════════════════════════════════════════════════
    // PRODUCT MODEL (Entity)
    // ════════════════════════════════════════════════════════════════════════
    // This class maps directly to the PRODUCTS table in Oracle DB.
    // EF Core reads this class and creates the table for you via Migrations.
    //
    // RELATIONSHIPS:
    //   Product ──→ User (Seller)     many products belong to one seller
    //   Product ──→ Category          many products belong to one category
    //   Product ──→ OrderItem[]       a product can be in many order lines
    //   Product ──→ Review[]          a product can have many reviews
    //
    // IMAGE STORAGE:
    //   Images are NOT stored in Oracle.
    //   They are uploaded to Cloudinary → Cloudinary returns a URL.
    //   We only save that URL string (ImageUrl) in this table.
    //
    // AFTER ADDING THIS FILE:
    //   dotnet ef migrations add AddProductTable
    //   dotnet ef database update
    // ════════════════════════════════════════════════════════════════════════

    [Table("PRODUCTS")]
    public class Product
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        // ── Core Information ─────────────────────────────────────────────

        /// <summary>Short display name shown in listings. E.g. "Nike Air Max 90"</summary>
        [Required]
        [MaxLength(200)]
        [Column("TITLE")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Full product description shown on detail page.</summary>
        [Required]
        [MaxLength(3000)]
        [Column("DESCRIPTION", TypeName = "NCLOB")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// URL-friendly version of the title.
        /// E.g. "nike-air-max-90" — used in product URLs /products/nike-air-max-90
        /// Generated automatically from Title in the ProductService.
        /// </summary>
        [Required]
        [MaxLength(250)]
        [Column("SLUG")]
        public string Slug { get; set; } = string.Empty;

        // ── Pricing ──────────────────────────────────────────────────────

        /// <summary>Original price before any discount. Must always be greater than 0.</summary>
        [Required]
        [Column("PRICE", TypeName = "DECIMAL(10,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Discount percentage. 0 = no discount. 20 = 20% off.
        /// Allowed range: 0 to 90.
        /// </summary>
        [Column("DISCOUNT_PERCENT", TypeName = "DECIMAL(5,2)")]
        public decimal DiscountPercent { get; set; } = 0;

        /// <summary>
        /// Final selling price after discount applied.
        /// [NotMapped] means this is computed in C# — NOT stored as a column in Oracle.
        /// Example: Price = 1000, DiscountPercent = 20 → FinalPrice = 800
        /// </summary>
        [NotMapped]
        public decimal FinalPrice => Price - (Price * DiscountPercent / 100);

        // ── Inventory ────────────────────────────────────────────────────

        /// <summary>
        /// How many units are in stock.
        /// Decremented automatically when an order is placed.
        /// When Stock = 0, product shows as "Out of Stock".
        /// </summary>
        [Column("STOCK")]
        public int Stock { get; set; } = 0;

        /// <summary>Unit of measurement. E.g. "piece", "kg", "litre", "pair", "box"</summary>
        [MaxLength(50)]
        [Column("UNIT")]
        public string Unit { get; set; } = "piece";

        // ── Images (Cloudinary URLs) ──────────────────────────────────────

        /// <summary>
        /// Main product image URL returned by Cloudinary after upload.
        /// Example: https://res.cloudinary.com/yourcloud/image/upload/v123456/products/abc.jpg
        /// Displayed in product cards and detail page.
        /// </summary>
        [MaxLength(1000)]
        [Column("IMAGE_URL")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Cloudinary public_id for the main image.
        /// Needed when you want to DELETE or REPLACE the image on Cloudinary.
        /// Example: "ecommerce/products/product_abc123"
        /// </summary>
        [MaxLength(500)]
        [Column("IMAGE_PUBLIC_ID")]
        public string? ImagePublicId { get; set; }

        // ── Status and Visibility ─────────────────────────────────────────

        /// <summary>
        /// Seller-controlled visibility toggle.
        /// false = product hidden from all browsing/search results.
        /// Sellers can deactivate their own products anytime.
        /// </summary>
        [Column("IS_ACTIVE")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Admin-controlled approval flag.
        /// false = Admin has hidden this product (policy violation, fake listing, etc.)
        /// A product must be both IsActive AND IsApproved to appear publicly.
        /// </summary>
        [Column("IS_APPROVED")]
        public bool IsApproved { get; set; } = true;

        /// <summary>
        /// Featured products appear on the homepage hero section.
        /// Only Admin can set this flag.
        /// </summary>
        [Column("IS_FEATURED")]
        public bool IsFeatured { get; set; } = false;

        // ── Foreign Keys ─────────────────────────────────────────────────

        /// <summary>
        /// ID of the Seller (User) who created and owns this product.
        /// Required — every product must have an owner.
        /// </summary>
        [Required]
        [Column("SELLER_ID")]
        public int SellerId { get; set; }

        /// <summary>
        /// ID of the Category this product belongs to.
        /// Optional (nullable) — a product can exist without a category.
        /// If the category is deleted, this becomes NULL (SetNull behaviour).
        /// </summary>
        [Column("CATEGORY_ID")]
        public int? CategoryId { get; set; }

        // ── Timestamps ───────────────────────────────────────────────────

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties (EF Core Relationships) ─────────────────
        // These are NOT columns — EF Core uses them to JOIN tables automatically.
        // Use .Include(p => p.Seller) in queries to load them.

        /// <summary>The Seller who created this product. Loaded via JOIN on SELLER_ID.</summary>
        [ForeignKey("SellerId")]
        public User Seller { get; set; } = null!;

        /// <summary>The Category this product belongs to. Null if uncategorized.</summary>
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        /// <summary>All order line items that include this product.</summary>
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        /// <summary>All customer reviews for this product.</summary>
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        // ── Computed Properties (never stored in Oracle) ──────────────────

        /// <summary>Average star rating (1-5) from all reviews. Returns 0 if no reviews.</summary>
        [NotMapped]
        public double AverageRating =>
            Reviews.Any() ? Math.Round(Reviews.Average(r => r.Rating), 1) : 0;

        /// <summary>Total number of customer reviews.</summary>
        [NotMapped]
        public int ReviewCount => Reviews.Count;

        /// <summary>True when Stock is greater than 0.</summary>
        [NotMapped]
        public bool InStock => Stock > 0;

        /// <summary>True when DiscountPercent is greater than 0.</summary>
        [NotMapped]
        public bool HasDiscount => DiscountPercent > 0;
    }
}