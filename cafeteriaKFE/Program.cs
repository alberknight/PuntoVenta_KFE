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
// We can then opt-out of authorization for specific endpoints
// like Login, Register, etc., by using the [AllowAnonymous] attribute.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// User and Role Seeding Logic
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<long>>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Define default roles
        string[] roleNames = { "Admin", "Customer" };
        
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Create the roles and seed them to the database
                await roleManager.CreateAsync(new IdentityRole<long>(roleName));
                logger.LogInformation($"Role '{roleName}' created.");
            }
        }

        // Seed a default admin user
        var adminEmail = "admin@cafeteria.com";
        var adminPassword = "Password123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Deleted = false
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                // Assign the 'Admin' role to the admin user
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Default admin user created and assigned to 'Admin' role.");
            }
            else
            {
                // Log errors if user creation fails
                foreach (var error in result.Errors)
                {
                    logger.LogError($"Error creating default admin user: {error.Description}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// End User Seeding Logic

// Configure the HTTP request pipeline.
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
