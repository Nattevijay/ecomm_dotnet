using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// Represents a customer's order.
    /// Maps to the ORDERS table in Oracle DB.
    /// One order has many OrderItems (one per product purchased).
    ///
    /// Order Status Flow:
    ///   Pending → Processing → Shipped → Delivered
    ///                       ↘ Cancelled
    /// </summary>
    [Table("ORDERS")]
    public class Order
    {
        // ─── Primary Key ────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        // ─── Status ──────────────────────────────────────────────────────
        // Valid values: "Pending", "Processing", "Shipped", "Delivered", "Cancelled"
        [Required]
        [MaxLength(20)]
        [Column("STATUS")]
        public string Status { get; set; } = "Pending";

        // ─── Pricing ─────────────────────────────────────────────────────
        // Total is calculated at order time and saved — do NOT recalculate
        // later since product prices may have changed
        [Required]
        [Column("TOTAL_AMOUNT", TypeName = "DECIMAL(12,2)")]
        public decimal TotalAmount { get; set; }

        // ─── Shipping Address ────────────────────────────────────────────
        [Required]
        [MaxLength(500)]
        [Column("SHIPPING_ADDRESS")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("SHIPPING_CITY")]
        public string ShippingCity { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("SHIPPING_PINCODE")]
        public string ShippingPincode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("SHIPPING_STATE")]
        public string ShippingState { get; set; } = string.Empty;

        // ─── Payment ─────────────────────────────────────────────────────
        // "COD", "Card", "UPI" etc.
        [MaxLength(50)]
        [Column("PAYMENT_METHOD")]
        public string PaymentMethod { get; set; } = "COD";

        // "Pending", "Paid", "Failed", "Refunded"
        [MaxLength(20)]
        [Column("PAYMENT_STATUS")]
        public string PaymentStatus { get; set; } = "Pending";

        [MaxLength(200)]
        [Column("PAYMENT_TRANSACTION_ID")]
        public string? PaymentTransactionId { get; set; }

        // ─── Notes ───────────────────────────────────────────────────────
        [MaxLength(1000)]
        [Column("CUSTOMER_NOTES")]
        public string? CustomerNotes { get; set; }

        // ─── Foreign Key ─────────────────────────────────────────────────
        // Which customer placed this order
        [Required]
        [Column("USER_ID")]
        public int UserId { get; set; }

        // ─── Timestamps ──────────────────────────────────────────────────
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // When the order was delivered (null until delivered)
        [Column("DELIVERED_AT")]
        public DateTime? DeliveredAt { get; set; }

        // ─── Navigation Properties ───────────────────────────────────────
        // The customer who placed this order
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        // All items in this order (list of products + quantities)
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    // ========================================================================

    /// <summary>
    /// Represents a single product line inside an Order.
    /// Maps to the ORDER_ITEMS table in Oracle DB.
    /// Example: Order #5 has 3 items: 2x Shirt, 1x Shoes, 3x Socks
    ///
    /// IMPORTANT: We save the price at order time (PriceAtPurchase)
    /// because the product's current price may change later.
    /// </summary>
    [Table("ORDER_ITEMS")]
    public class OrderItem
    {
        // ─── Primary Key ────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        // ─── Quantity & Price ────────────────────────────────────────────
        [Required]
        [Column("QUANTITY")]
        public int Quantity { get; set; }

        // The price of ONE unit at the moment of purchase — frozen forever
        [Required]
        [Column("PRICE_AT_PURCHASE", TypeName = "DECIMAL(10,2)")]
        public decimal PriceAtPurchase { get; set; }

        // Total for this line: Quantity × PriceAtPurchase (not stored — computed)
        [NotMapped]
        public decimal LineTotal => Quantity * PriceAtPurchase;

        // ─── Foreign Keys ────────────────────────────────────────────────
        [Required]
        [Column("ORDER_ID")]
        public int OrderId { get; set; }

        [Required]
        [Column("PRODUCT_ID")]
        public int ProductId { get; set; }

        // ─── Navigation Properties ───────────────────────────────────────
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}