using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// Represents a product review written by a Customer.
    /// Maps to the REVIEWS table in Oracle DB.
    /// A customer can only review a product they have purchased.
    /// Rating is 1 to 5 stars.
    /// </summary>
    [Table("REVIEWS")]
    public class Review
    {
        // ─── Primary Key ────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        // ─── Review Content ──────────────────────────────────────────────
        // 1 = worst, 5 = best
        [Required]
        [Range(1, 5)]
        [Column("RATING")]
        public int Rating { get; set; }

        [MaxLength(200)]
        [Column("TITLE")]
        public string? Title { get; set; }

        [MaxLength(2000)]
        [Column("COMMENT")]
        public string? Comment { get; set; }

        // ─── Foreign Keys ────────────────────────────────────────────────
        [Required]
        [Column("USER_ID")]
        public int UserId { get; set; }

        [Required]
        [Column("PRODUCT_ID")]
        public int ProductId { get; set; }

        // ─── Timestamps ──────────────────────────────────────────────────
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ─── Navigation Properties ───────────────────────────────────────
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}