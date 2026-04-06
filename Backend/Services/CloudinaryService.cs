using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Backend.Helpers
{
    // ════════════════════════════════════════════════════════════════════════
    // CLOUDINARY SERVICE
    // ════════════════════════════════════════════════════════════════════════
    // Wraps the CloudinaryDotNet SDK for uploading and deleting images.
    //
    // HOW IT WORKS:
    //   1. Seller uploads product image via the API (multipart/form-data)
    //   2. ProductService calls CloudinaryService.UploadProductImageAsync()
    //   3. Cloudinary stores the image on their CDN servers
    //   4. Cloudinary returns a secure HTTPS URL + a public_id
    //   5. We save the URL in Product.ImageUrl and public_id in Product.ImagePublicId
    //   6. React displays the image using the URL directly from Cloudinary CDN
    //   7. When product is deleted/updated → DeleteImageAsync(publicId) removes it
    //
    // REGISTER in Program.cs:
    //   builder.Services.AddScoped<CloudinaryService>();
    //
    // CREDENTIALS in appsettings.json:
    //   "Cloudinary": {
    //     "CloudName": "your-cloud-name",
    //     "ApiKey":    "your-api-key",
    //     "ApiSecret": "your-api-secret"
    //   }
    // ════════════════════════════════════════════════════════════════════════

    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // Always use HTTPS URLs
        }

        // ════════════════════════════════════════════════════════════════════
        // UPLOAD PRODUCT IMAGE
        // ════════════════════════════════════════════════════════════════════
        /// <summary>
        /// Uploads a product image to Cloudinary.
        /// Automatically resizes to max 800px wide, optimizes quality.
        /// Returns the secure URL and public_id.
        /// </summary>
        public async Task<CloudinaryUploadResult> UploadProductImageAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                // The file stream + original filename
                File = new FileDescription(file.FileName, stream),

                // Store all product images in an "ecommerce/products" folder
                Folder = "ecommerce/products",

                // Auto-generate a unique public_id (don't use the original filename)
                UniqueFilename = true,
                Overwrite      = false,

                // Transformations applied on upload:
                // - limit width to 800px (preserve aspect ratio)
                // - auto quality (Cloudinary picks the best compression)
                // - auto format (serves WebP to browsers that support it)
                Transformation = new Transformation()
                    .Width(800)
                    .Crop("limit")
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

            return new CloudinaryUploadResult
            {
                SecureUrl = result.SecureUrl.ToString(),
                PublicId  = result.PublicId
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // UPLOAD PROFILE IMAGE
        // ════════════════════════════════════════════════════════════════════
        /// <summary>
        /// Uploads a user profile/avatar image.
        /// Crops to square 300x300 for consistent avatar display.
        /// </summary>
        public async Task<CloudinaryUploadResult> UploadProfileImageAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File           = new FileDescription(file.FileName, stream),
                Folder         = "ecommerce/profiles",
                UniqueFilename = true,
                Transformation = new Transformation()
                    .Width(300).Height(300)
                    .Crop("fill")          // crop to exact 300x300 square
                    .Gravity("face")       // focus crop on the face if detected
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

            return new CloudinaryUploadResult
            {
                SecureUrl = result.SecureUrl.ToString(),
                PublicId  = result.PublicId
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // DELETE IMAGE
        // ════════════════════════════════════════════════════════════════════
        /// <summary>
        /// Permanently deletes an image from Cloudinary using its public_id.
        /// Called when a product is deleted or its image is replaced.
        /// Does nothing if publicId is null or empty.
        /// </summary>
        public async Task DeleteImageAsync(string? publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return;

            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);

            // Log but do not throw — deletion failure is non-critical
            if (result.Result != "ok")
                Console.WriteLine($"Cloudinary deletion warning for {publicId}: {result.Result}");
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // RESULT DTO
    // ════════════════════════════════════════════════════════════════════════
    /// <summary>
    /// What Cloudinary returns after a successful upload.
    /// SecureUrl → save in Product.ImageUrl
    /// PublicId  → save in Product.ImagePublicId (needed for deletion later)
    /// </summary>
    public class CloudinaryUploadResult
    {
        public string SecureUrl { get; set; } = string.Empty;
        public string PublicId  { get; set; } = string.Empty;
    }
}