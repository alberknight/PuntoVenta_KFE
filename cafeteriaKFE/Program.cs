using cafeteriaKFE.Data;
using cafeteriaKFE.Models; // Ensure User model is accessible
using cafeteriaKFE.Repository;
using cafeteriaKFE.Repository.Catalogs;
using cafeteriaKFE.Repository.Orders;
using cafeteriaKFE.Repository.Products;
using cafeteriaKFE.Repository.Home;
using cafeteriaKFE.Services;
using Microsoft.AspNetCore.Authorization; // Added for DataProtection
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity; // Added for Identity
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // Added for CreateScope
using System; // Added for TimeSpan
using System.IO; // Added for Path and DirectoryInfo

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPosService, PosService>();
builder.Services.AddScoped<IPosCatalogRepository, PosCatalogRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();


// Configure Data Protection to persist keys to the file system
// This prevents "key not found" errors on app restart for antiforgery tokens etc.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole<long>>(options =>
{
    // Configure Identity options
    options.SignIn.RequireConfirmedAccount = false; // For simplicity, can be set to true for email confirmation
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6; // Minimum password length
})
.AddEntityFrameworkStores<PosDbContext>()
.AddDefaultTokenProviders();

// Configure Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout"; // Ensure a logout path is defined
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie valid for 60 minutes
});

builder.Services.AddControllersWithViews();

// Set up a fallback authorization policy.
// This requires all endpoints to be authorized by default.
// like Login, Register, etc., by using the [AllowAnonymous] attribute.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();


// Configure the HTTP request p ipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication and Authorization middleware must be between UseRouting and MapControllerRoute
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "product",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
