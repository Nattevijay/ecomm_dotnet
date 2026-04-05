using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// Represents a product category.
    /// Maps to the CATEGORIES table in Oracle DB.
    /// Example: Electronics, Clothing, Books
    /// Created only by Admin.
    /// </summary>
    [Table("CATEGORIES")]
    public class Category
    {
        // ─── Primary Key ────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        // ─── Info ────────────────────────────────────────────────────────
        [Required]
        [MaxLength(100)]
        [Column("NAME")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("DESCRIPTION")]
        public string? Description { get; set; }

        // Slug is a URL-friendly name: "electronics", "mens-clothing"
        [Required]
        [MaxLength(120)]
        [Column("SLUG")]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("IMAGE_URL")]
        public string? ImageUrl { get; set; }

        [Column("IS_ACTIVE")]
        public bool IsActive { get; set; } = true;

        // ─── Timestamps ──────────────────────────────────────────────────
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ─── Navigation ──────────────────────────────────────────────────
        // One category has many products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}