using Backend.DTOs;
using FluentValidation;

namespace Backend.Validators
{
    // ════════════════════════════════════════════════════════════════════════
    // PRODUCT VALIDATORS (FluentValidation)
    // ════════════════════════════════════════════════════════════════════════
    // FluentValidation runs automatically BEFORE the controller action.
    // If any rule fails → controller returns 400 Bad Request with all
    // error messages listed — you never need manual if/else checks.
    //
    // HOW IT WORKS:
    //   1. Seller submits POST /api/products with CreateProductDto body
    //   2. FluentValidation intercepts and runs CreateProductDtoValidator
    //   3. If all rules pass → controller action runs
    //   4. If any rule fails → returns 400 with { "errors": { "title": [...] } }
    //
    // VALIDATORS IN THIS FILE:
    //   CreateProductDtoValidator  ← rules for creating a new product
    //   UpdateProductDtoValidator  ← rules for editing an existing product
    //   ProductFilterDtoValidator  ← rules for search/filter query params
    // ════════════════════════════════════════════════════════════════════════


    // ── Validator 1: Create Product ──────────────────────────────────────
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            // ── TITLE ─────────────────────────────────────────────────────
            RuleFor(x => x.Title)
                .NotEmpty()
                    .WithMessage("Product title is required")
                .MinimumLength(3)
                    .WithMessage("Title must be at least 3 characters long")
                .MaximumLength(200)
                    .WithMessage("Title cannot exceed 200 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_',.()\[\]&]+$")
                    .WithMessage("Title contains invalid characters");

            // ── DESCRIPTION ───────────────────────────────────────────────
            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage("Product description is required")
                .MinimumLength(20)
                    .WithMessage("Description must be at least 20 characters — help customers understand the product")
                .MaximumLength(3000)
                    .WithMessage("Description cannot exceed 3000 characters");

            // ── PRICE ─────────────────────────────────────────────────────
            RuleFor(x => x.Price)
                .GreaterThan(0)
                    .WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(9999999)
                    .WithMessage("Price cannot exceed 9,999,999")
                .Must(price => decimal.Round(price, 2) == price)
                    .WithMessage("Price can have at most 2 decimal places");

            // ── DISCOUNT PERCENT ──────────────────────────────────────────
            RuleFor(x => x.DiscountPercent)
                .GreaterThanOrEqualTo(0)
                    .WithMessage("Discount cannot be negative")
                .LessThanOrEqualTo(90)
                    .WithMessage("Discount cannot exceed 90%")
                .Must(d => decimal.Round(d, 2) == d)
                    .WithMessage("Discount can have at most 2 decimal places");

            // ── STOCK ─────────────────────────────────────────────────────
            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0)
                    .WithMessage("Stock cannot be negative")
                .LessThanOrEqualTo(999999)
                    .WithMessage("Stock cannot exceed 999,999 units");

            // ── UNIT ──────────────────────────────────────────────────────
            RuleFor(x => x.Unit)
                .NotEmpty()
                    .WithMessage("Unit is required (e.g. piece, kg, litre, pair)")
                .MaximumLength(50)
                    .WithMessage("Unit cannot exceed 50 characters")
                .Must(unit => ValidUnits.Contains(unit.ToLower()))
                    .WithMessage($"Unit must be one of: {string.Join(", ", ValidUnits)}");

            // ── CATEGORY (optional) ───────────────────────────────────────
            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                    .WithMessage("Category ID must be a positive number")
                .When(x => x.CategoryId.HasValue);  // Only validate IF provided
        }

        // Valid unit values — lowercase comparison
        private static readonly HashSet<string> ValidUnits = new(StringComparer.OrdinalIgnoreCase)
        {
            "piece", "pair", "set", "pack", "box",
            "kg", "g", "mg",
            "litre", "ml",
            "metre", "cm", "mm",
            "dozen", "unit"
        };
    }


    // ── Validator 2: Update Product ──────────────────────────────────────
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            // ── TITLE ─────────────────────────────────────────────────────
            RuleFor(x => x.Title)
                .NotEmpty()
                    .WithMessage("Product title is required")
                .MinimumLength(3)
                    .WithMessage("Title must be at least 3 characters long")
                .MaximumLength(200)
                    .WithMessage("Title cannot exceed 200 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_',.()\[\]&]+$")
                    .WithMessage("Title contains invalid characters");

            // ── DESCRIPTION ───────────────────────────────────────────────
            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage("Product description is required")
                .MinimumLength(20)
                    .WithMessage("Description must be at least 20 characters")
                .MaximumLength(3000)
                    .WithMessage("Description cannot exceed 3000 characters");

            // ── PRICE ─────────────────────────────────────────────────────
            RuleFor(x => x.Price)
                .GreaterThan(0)
                    .WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(9999999)
                    .WithMessage("Price cannot exceed 9,999,999")
                .Must(price => decimal.Round(price, 2) == price)
                    .WithMessage("Price can have at most 2 decimal places");

            // ── DISCOUNT PERCENT ──────────────────────────────────────────
            RuleFor(x => x.DiscountPercent)
                .GreaterThanOrEqualTo(0)
                    .WithMessage("Discount cannot be negative")
                .LessThanOrEqualTo(90)
                    .WithMessage("Discount cannot exceed 90%")
                .Must(d => decimal.Round(d, 2) == d)
                    .WithMessage("Discount can have at most 2 decimal places");

            // ── STOCK ─────────────────────────────────────────────────────
            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0)
                    .WithMessage("Stock cannot be negative")
                .LessThanOrEqualTo(999999)
                    .WithMessage("Stock cannot exceed 999,999 units");

            // ── UNIT ──────────────────────────────────────────────────────
            RuleFor(x => x.Unit)
                .NotEmpty()
                    .WithMessage("Unit is required")
                .MaximumLength(50)
                    .WithMessage("Unit cannot exceed 50 characters");

            // ── CATEGORY (optional) ───────────────────────────────────────
            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                    .WithMessage("Category ID must be a positive number")
                .When(x => x.CategoryId.HasValue);
        }
    }


    // ── Validator 3: Product Filter Query Params ─────────────────────────
    public class ProductFilterDtoValidator : AbstractValidator<ProductFilterDto>
    {
        public ProductFilterDtoValidator()
        {
            // ── SEARCH ────────────────────────────────────────────────────
            RuleFor(x => x.Search)
                .MaximumLength(100)
                    .WithMessage("Search term cannot exceed 100 characters")
                .When(x => x.Search != null);

            // ── PRICE RANGE ───────────────────────────────────────────────
            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0)
                    .WithMessage("Minimum price cannot be negative")
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.MaxPrice)
                .GreaterThan(0)
                    .WithMessage("Maximum price must be greater than 0")
                .When(x => x.MaxPrice.HasValue);

            // Min price must be less than max price when both are provided
            RuleFor(x => x)
                .Must(x => x.MinPrice < x.MaxPrice)
                    .WithMessage("Minimum price must be less than maximum price")
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

            // ── SORT BY ───────────────────────────────────────────────────
            RuleFor(x => x.SortBy)
                .Must(sort => ValidSortOptions.Contains(sort))
                    .WithMessage($"SortBy must be one of: {string.Join(", ", ValidSortOptions)}");

            // ── PAGINATION ────────────────────────────────────────────────
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                    .WithMessage("Page must be 1 or greater");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                    .WithMessage("PageSize must be at least 1")
                .LessThanOrEqualTo(50)
                    .WithMessage("PageSize cannot exceed 50 items per page");
        }

        private static readonly HashSet<string> ValidSortOptions = new(StringComparer.OrdinalIgnoreCase)
        {
            "newest", "oldest", "price_asc", "price_desc", "rating", "popular"
        };
    }
}