using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CATEGORIES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    SLUG = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    IMAGE_URL = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORIES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    PASSWORD_HASH = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    ROLE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false, defaultValue: "Customer"),
                    IS_BANNED = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false),
                    BAN_REASON = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    PROFILE_IMAGE_URL = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    PHONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ORDERS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "DECIMAL(12,2)", nullable: false),
                    SHIPPING_ADDRESS = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    SHIPPING_CITY = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SHIPPING_PINCODE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    SHIPPING_STATE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PAYMENT_METHOD = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false, defaultValue: "COD"),
                    PAYMENT_STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    PAYMENT_TRANSACTION_ID = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CUSTOMER_NOTES = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    USER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DELIVERED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ORDERS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCTS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TITLE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NCLOB", maxLength: 3000, nullable: false),
                    SLUG = table.Column<string>(type: "NVARCHAR2(250)", maxLength: 250, nullable: false),
                    PRICE = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    DISCOUNT_PERCENT = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    STOCK = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UNIT = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    IMAGE_URL = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    IMAGE_PUBLIC_ID = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: true),
                    IS_APPROVED = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: true),
                    IS_FEATURED = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    SELLER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CATEGORY_ID = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CATEGORIES_CATEGORY_ID",
                        column: x => x.CATEGORY_ID,
                        principalTable: "CATEGORIES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_USERS_SELLER_ID",
                        column: x => x.SELLER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ORDER_ITEMS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    QUANTITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PRICE_AT_PURCHASE = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    ORDER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PRODUCT_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_ITEMS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ORDER_ITEMS_ORDERS_ORDER_ID",
                        column: x => x.ORDER_ID,
                        principalTable: "ORDERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ORDER_ITEMS_PRODUCTS_PRODUCT_ID",
                        column: x => x.PRODUCT_ID,
                        principalTable: "PRODUCTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REVIEWS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RATING = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TITLE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    COMMENT = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    USER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PRODUCT_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REVIEWS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REVIEWS_PRODUCTS_PRODUCT_ID",
                        column: x => x.PRODUCT_ID,
                        principalTable: "PRODUCTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REVIEWS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_CATEGORIES_SLUG",
                table: "CATEGORIES",
                column: "SLUG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_ITEMS_ORDER_ID",
                table: "ORDER_ITEMS",
                column: "ORDER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_ITEMS_PRODUCT_ID",
                table: "ORDER_ITEMS",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_STATUS",
                table: "ORDERS",
                column: "STATUS");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_USER_ID",
                table: "ORDERS",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CATEGORY_ID",
                table: "PRODUCTS",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_SELLER_ID",
                table: "PRODUCTS",
                column: "SELLER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_REVIEWS_PRODUCT_ID",
                table: "REVIEWS",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "UX_REVIEWS_USER_PRODUCT",
                table: "REVIEWS",
                columns: new[] { "USER_ID", "PRODUCT_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_USERS_EMAIL",
                table: "USERS",
                column: "EMAIL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ORDER_ITEMS");

            migrationBuilder.DropTable(
                name: "REVIEWS");

            migrationBuilder.DropTable(
                name: "ORDERS");

            migrationBuilder.DropTable(
                name: "PRODUCTS");

            migrationBuilder.DropTable(
                name: "CATEGORIES");

            migrationBuilder.DropTable(
                name: "USERS");
        }
    }
}
