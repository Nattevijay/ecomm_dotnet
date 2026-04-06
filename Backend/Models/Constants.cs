namespace Backend.Models
{
    /// <summary>
    /// Constants for Order Status values.
    /// Use these instead of typing strings directly — avoids typos.
    ///
    /// Usage:
    ///   order.Status = OrderStatus.Pending;
    ///   if (order.Status == OrderStatus.Delivered) { ... }
    /// </summary>
    public static class OrderStatus
    {
        public const string Pending    = "Pending";
        public const string Processing = "Processing";
        public const string Shipped    = "Shipped";
        public const string Delivered  = "Delivered";
        public const string Cancelled  = "Cancelled";

        // All valid statuses — useful for validation
        public static readonly string[] All =
        {
            Pending, Processing, Shipped, Delivered, Cancelled
        };
    }

    /// <summary>
    /// Constants for Payment Status values.
    /// </summary>
    public static class PaymentStatus
    {
        public const string Pending  = "Pending";
        public const string Paid     = "Paid";
        public const string Failed   = "Failed";
        public const string Refunded = "Refunded";
    }

    /// <summary>
    /// Constants for User Role values.
    /// Use these everywhere instead of hardcoding "Admin", "Seller" etc.
    ///
    /// Usage in Controller:
    ///   [Authorize(Roles = UserRoles.Admin)]
    ///   [Authorize(Roles = UserRoles.Seller + "," + UserRoles.Admin)]
    /// </summary>
    public static class UserRoles
    {
        public const string Customer = "Customer";
        public const string Seller   = "Seller";
        public const string Admin    = "Admin";
    }

    /// <summary>
    /// Constants for Payment Methods.
    /// </summary>
    public static class PaymentMethod
    {
        public const string COD  = "COD";
        public const string Card = "Card";
        public const string UPI  = "UPI";
    }
}