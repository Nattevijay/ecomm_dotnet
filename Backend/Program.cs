using System.Text;
using Backend.Data;
using Backend.Helpers;
using Backend.Middleware;
using Backend.Repositories;
using Backend.Repositories.Interfaces;
using Backend.Services;
using Backend.Services.Interfaces;
using Backend.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// ════════════════════════════════════════════════════════════════════════════
// This is the entry point of your entire backend app.
// Everything is registered here:
//   - Database (Oracle via EF Core)
//   - Services, Repositories (dependency injection)
//   - JWT Authentication
//   - CORS (so React can call this API)
//   - Swagger (interactive API docs)
//   - FluentValidation
//   - Middleware (global error handler)
//
// ORDER MATTERS: The pipeline runs top to bottom for every request.
// ═════

// builder.Services.AddOpenApi();

// ── 1. ADD CONTROLLERS ───────────────────────────────────────────────────
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration
      .GetConnectionString("OracleDB")));


      // Format: AddScoped<Interface, Implementation>
// "Scoped" means: one instance per HTTP request (recommended for most things)
 
// Repositories (Layer 1 — DB access)
builder.Services.AddScoped<IUserRepository, UserRepository>();
 
// Services (Layer 2 — Business logic)
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<CloudinaryService>();
 
// Helpers
builder.Services.AddScoped<JwtService>();


// ── 4. FLUENT VALIDATION ─────────────────────────────────────────────────
// Automatically validates DTOs before controller actions run
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
// Register all validators from this assembly (finds RegisterDtoValidator etc.)
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();



// ── 5. JWT AUTHENTICATION ────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,   // Rejects expired tokens
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtIssuer,
        ValidAudience            = jwtAudience,
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew                = TimeSpan.Zero  // No extra grace period
    };
 
    // Return proper 401 JSON (not a redirect to login page)
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode  = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"error\": \"You must be logged in to access this resource\"}");
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode  = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"error\": \"You do not have permission to access this resource\"}");
        }
    };
});
 
builder.Services.AddAuthorization();


// ── 6. CORS — Allow React Frontend to Call This API ──────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",  // Vite dev server
                "http://localhost:3000"   // Alternative React port
            )
            .AllowAnyHeader()     // Allow Authorization, Content-Type etc.
            .AllowAnyMethod()     // Allow GET, POST, PUT, DELETE
            .AllowCredentials();  // Allow cookies if needed later
    });
});
      


// ── 7. SWAGGER — Interactive API Documentation ───────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ecommerce API",
        Version = "v1"
    });

    // 🔥 ADD THIS BLOCK
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter token like: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

 
// ── 8. LOGGING ───────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();



app.MapGet("/",() =>
{
    return "Api Up and Running...";
});



// ── CONFIGURE MIDDLEWARE PIPELINE ────────────────────────────────────────
// ORDER IS CRITICAL — these run in the exact order listed below for every request
 
// 1. Global error handler — MUST be first to catch errors from any middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
 
// 2. Swagger UI (only in development)
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
// }
 
// 3. CORS — must be before Authentication
app.UseCors("ReactPolicy");
 
// 4. HTTPS redirect
app.UseHttpsRedirection();
 
// 5. Authentication (reads JWT token from Authorization header)
app.UseAuthentication();
 
// 6. Authorization (checks [Authorize] attributes on controllers)
app.UseAuthorization();
 
// 7. Map controllers (routes requests to the right controller action)
app.MapControllers();
 
// ── START THE APP ────────────────────────────────────────────────────────
app.Run();
// Now listening at: http://localhost:5000
// Swagger UI at:    http://localhost:5000/swagger