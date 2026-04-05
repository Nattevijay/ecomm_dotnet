using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// Maps to the USERS table in Oracle DB.
    /// Roles: "Customer", "Seller", "Admin"
    /// </summary>
    [Table("USERS")]
    public class User
    {
        // ─── Primary Key ────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        // ─── Basic Info ──────────────────────────────────────────────────
        [Required]
        [MaxLength(100)]
        [Column("NAME")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("EMAIL")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("PASSWORD_HASH")]
        public string PasswordHash { get; set; } = string.Empty;

        // ─── Role ────────────────────────────────────────────────────────
        // Possible values: "Customer", "Seller", "Admin"
        [Required]
        [MaxLength(20)]
        [Column("ROLE")]
        public string Role { get; set; } = "Customer";

        // ─── Account Status ──────────────────────────────────────────────
        // Admin can ban users — banned users cannot log in
        [Column("IS_BANNED")]
        public bool IsBanned { get; set; } = false;

        [MaxLength(500)]
        [Column("BAN_REASON")]
        public string? BanReason { get; set; }

        // ─── Profile ─────────────────────────────────────────────────────
        [MaxLength(500)]
        [Column("PROFILE_IMAGE_URL")]
        public string? ProfileImageUrl { get; set; }

        [MaxLength(20)]
        [Column("PHONE")]
        public string? Phone { get; set; }

        // ─── Timestamps ──────────────────────────────────────────────────
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ─── Navigation Properties (Relationships) ───────────────────────
        // One user can have many products (if they are a Seller)
        public ICollection<Product> Products { get; set; } = new List<Product>();

        // One user can place many orders
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        // One user can write many reviews
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}