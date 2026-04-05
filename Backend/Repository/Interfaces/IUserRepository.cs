using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYER 1: REPOSITORY INTERFACE
    // ════════════════════════════════════════════════════════════════════════
    // This interface defines WHAT operations we can do with users in the DB.
    // It does NOT say HOW — that is the job of UserRepository.cs.
    //
    // WHY AN INTERFACE?
    //   - The Service layer depends on THIS interface, not the concrete class.
    //   - This means you can swap Oracle for a different DB later — zero changes
    //     to your service or controller code.
    //   - Also makes unit testing easy (you can mock this interface).
    //
    // REGISTER in Program.cs:
    //   builder.Services.AddScoped<IUserRepository, UserRepository>();
    // ════════════════════════════════════════════════════════════════════════

    public interface IUserRepository
    {
        // ── Read Operations ──────────────────────────────────────────────

        /// <summary>Find a user by their database ID. Returns null if not found.</summary>
        Task<User?> GetByIdAsync(int id);

        /// <summary>Find a user by email address. Returns null if not found.</summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>Get ALL users in the system (Admin use only).</summary>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>Check if an email is already registered. Returns true if exists.</summary>
        Task<bool> EmailExistsAsync(string email);

        // ── Write Operations ─────────────────────────────────────────────

        /// <summary>Save a brand new user to the database.</summary>
        Task<User> AddAsync(User user);

        /// <summary>Update an existing user's data in the database.</summary>
        Task<User> UpdateAsync(User user);

        /// <summary>Delete a user from the database (Admin only).</summary>
        Task DeleteAsync(int id);
    }
}