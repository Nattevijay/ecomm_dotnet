using Backend.Data;
using Backend.DTOs;
using Backend.Exceptions;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYER 1: PRODUCT REPOSITORY (Implementation)
    // ════════════════════════════════════════════════════════════════════════
    // All Oracle database operations for products.
    // Uses Entity Framework Core — writes C#, EF generates the Oracle SQL.
    //
    // KEY CONCEPTS:
    //   .Include()        → JOIN with another table (load related data)
    //   .Where()          → SQL WHERE clause (filter rows)
    //   .OrderBy()        → SQL ORDER BY
    //   .Skip().Take()    → SQL OFFSET / FETCH NEXT (pagination)
    //   .AsNoTracking()   → Read-only queries (faster — no change tracking)
    // ════════════════════════════════════════════════════════════════════════

    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        // ════════════════════════════════════════════════════════════════════
        // GET BY ID
        // ════════════════════════════════════════════════════════════════════
        public async Task<Product?> GetByIdAsync(int id)
        {
            // .Include() = LEFT JOIN so we load Seller + Category + Reviews in one query
            return await _context.Products
                .Include(p => p.Seller)     // JOIN USERS ON SELLER_ID
                .Include(p => p.Category)   // JOIN CATEGORIES ON CATEGORY_ID
                .Include(p => p.Reviews)    // JOIN REVIEWS ON PRODUCT_ID
                    .ThenInclude(r => r.User) // also load reviewer's name
                .AsNoTracking()             // read-only = faster
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // ════════════════════════════════════════════════════════════════════
        // GET BY SLUG
        // ════════════════════════════════════════════════════════════════════
        public async Task<Product?> GetBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Seller)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug == slug.ToLower());
        }

        // ════════════════════════════════════════════════════════════════════
        // GET ALL (with filter, sort, pagination)
        // ════════════════════════════════════════════════════════════════════
        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
            ProductFilterDto filter)
        {
            // Start with a base query — IQueryable lets us chain filters
            // before hitting the database (only ONE SQL query is sent)
            var query = _context.Products
                .Include(p => p.Seller)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .AsNoTracking()
                .AsQueryable();

            // ── Apply Visibility Filters ─────────────────────────────────
            // Public browsing: only show active AND approved products
            // (Admin can override this in their own admin endpoint)
            query = query.Where(p => p.IsActive && p.IsApproved);

            // ── Apply Search Filter ──────────────────────────────────────
            // Case-insensitive search across title AND description
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchLower = filter.Search.ToLower().Trim();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(searchLower) ||
                    p.Description.ToLower().Contains(searchLower));
            }

            // ── Apply Category Filter ────────────────────────────────────
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            // ── Apply Seller Filter (for Seller Dashboard) ───────────────
            if (filter.SellerId.HasValue)
            {
                query = query.Where(p => p.SellerId == filter.SellerId.Value);
            }

            // ── Apply Price Range Filter ─────────────────────────────────
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            // ── Apply Stock Filter ───────────────────────────────────────
            if (filter.InStockOnly == true)
            {
                query = query.Where(p => p.Stock > 0);
            }

            // ── Apply Featured Filter ────────────────────────────────────
            if (filter.FeaturedOnly == true)
            {
                query = query.Where(p => p.IsFeatured);
            }

            // ── Count BEFORE pagination (for TotalPages calculation) ─────
            var totalCount = await query.CountAsync();

            // ── Apply Sorting ────────────────────────────────────────────
            query = filter.SortBy.ToLower() switch
            {
                "oldest" => query.OrderBy(p => p.CreatedAt),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p =>
                                    p.Reviews.Any()
                                    ? p.Reviews.Average(r => r.Rating)
                                    : 0),
                "popular" => query.OrderByDescending(p => p.Reviews.Count),
                _ => query.OrderByDescending(p => p.CreatedAt) // "newest" = default
            };

            // ── Apply Pagination ─────────────────────────────────────────
            // Page 1 → skip 0,  take 12
            // Page 2 → skip 12, take 12
            // Page 3 → skip 24, take 12
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);
            var skip = (filter.Page - 1) * pageSize;

            var products = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        // ════════════════════════════════════════════════════════════════════
        // GET BY SELLER ID
        // ════════════════════════════════════════════════════════════════════
        public async Task<IEnumerable<Product>> GetBySellerIdAsync(int sellerId)
        {
            // No IsActive filter — sellers can see ALL their products including hidden ones
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .AsNoTracking()
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ════════════════════════════════════════════════════════════════════
        // SLUG EXISTS CHECK
        // ════════════════════════════════════════════════════════════════════
        public async Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null)
        {
            var query = _context.Products.Where(p => p.Slug == slug.ToLower());

            // When updating: exclude the current product from the check
            if (excludeProductId.HasValue)
                query = query.Where(p => p.Id != excludeProductId.Value);

            return await query.AnyAsync();
        }

        // ════════════════════════════════════════════════════════════════════
        // ADD (INSERT)
        // // ════════════════════════════════════════════════════════════════════
        // public async Task<Product> AddAsync(Product product)
        // {
        //     await _context.Products.AddAsync(product);
        //     await _context.SaveChangesAsync();
        //     // After save, product.Id is now set by Oracle (auto-increment)
        //     return product;
        // }

        public async Task<Product> AddAsync(Product product)
        {
            try
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 DB ERROR: " + ex.InnerException?.Message);
                throw;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // UPDATE
        // ════════════════════════════════════════════════════════════════════
        public async Task<Product> UpdateAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        // ════════════════════════════════════════════════════════════════════
        // DELETE
        // ════════════════════════════════════════════════════════════════════
        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                throw new NotFoundException("Product", id);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}