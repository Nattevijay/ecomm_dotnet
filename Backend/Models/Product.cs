using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// Represents a product listed by a Seller.
    /// Maps to the PRODUCTS table in Oracle DB.
    /// Images are stored in Cloudinary — only the URL is saved here.
    /// </summary>
    [Table("PRODUCTS")]
    public class Product
    {
        // ─── Primary Key ────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        // ─── Core Info ───────────────────────────────────────────────────
        [Required]
        [MaxLength(200)]
        [Column("TITLE")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        [Column("DESCRIPTION")]
        public string Description { get; set; } = string.Empty;

        // ─── Pricing ─────────────────────────────────────────────────────
        [Required]
        [Column("PRICE", TypeName = "DECIMAL(10,2)")]
        public decimal Price { get; set; }

        // Discount percentage: 0 means no discount, 20 means 20% off
        [Column("DISCOUNT_PERCENT")]
        public decimal DiscountPercent { get; set; } = 0;

        // Computed: actual selling price after discount (not stored in DB)
        [NotMapped]
        public decimal FinalPrice =>
            Price - (Price * DiscountPercent / 100);

        // ─── Inventory ───────────────────────────────────────────────────
        [Column("STOCK")]
        public int Stock { get; set; } = 0;

        // ─── Images (Cloudinary URLs) ────────────────────────────────────
        // Main product image URL from Cloudinary
        [MaxLength(1000)]
        [Column("IMAGE_URL")]
        public string? ImageUrl { get; set; }

        // Cloudinary public_id — needed if you want to DELETE the image later
        [MaxLength(500)]
        [Column("IMAGE_PUBLIC_ID")]
        public string? ImagePublicId { get; set; }

        // ─── Status ──────────────────────────────────────────────────────
        [Column("IS_ACTIVE")]
        public bool IsActive { get; set; } = true;

        // Admin can hide products that violate rules
        [Column("IS_APPROVED")]
        public bool IsApproved { get; set; } = true;

        // ─── Foreign Keys ────────────────────────────────────────────────
        // Which seller created this product
        [Required]
        [Column("SELLER_ID")]
        public int SellerId { get; set; }

        // Which category this product belongs to
        [Column("CATEGORY_ID")]
        public int? CategoryId { get; set; }

        // ─── Timestamps ──────────────────────────────────────────────────
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ─── Navigation Properties ───────────────────────────────────────
        // The seller (User) who created this product
        [ForeignKey("SellerId")]
        public User Seller { get; set; } = null!;

        // The category this product belongs to
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        // A product can appear in many order items
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // A product can have many reviews
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        // ─── Computed from Reviews (not stored) ──────────────────────────
        [NotMapped]
        public double AverageRating =>
            Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;

        [NotMapped]
        public int ReviewCount => Reviews.Count;
    }
}