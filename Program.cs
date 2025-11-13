using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc.Infrastructure; // Required for IActionContextAccessor
using Microsoft.AspNetCore.Mvc.Routing; // Required for IUrlHelperFactory
using Microsoft.AspNetCore.Mvc; // Required for IUrlHelper and ActionContext

var builder = WebApplication.CreateBuilder(args);

// ====================================
// SERVICES CONFIGURATION
// ====================================

// ✅ Email Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddTransient<IEmailSender, EmailService>();

// 🎯 REQUIRED FOR IUrlHelper IN API CONTROLLER (To generate Password Reset Link)
// NOTE: For services generating links outside of a Controller, consider injecting LinkGenerator (IUrlHelper is designed for Controller context).
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

// 🛠️ Structural Fix: Simplified IUrlHelper registration. 
// It now correctly attempts to resolve the current ActionContext or uses an empty one as a fallback.
builder.Services.AddScoped<IUrlHelper>(x =>
{
    var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext ?? new ActionContext();
    return x.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
});

// ✅ MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ✅ JWT Token Generator
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// ✅ Bind Services
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);
builder.Services.AddScoped<IAuthService, AuthService>();

// ✅ Configure MySQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// ====================================
// IDENTITY CONFIGURATION
// ====================================

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ✅ Configure Identity Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Index";
    options.AccessDeniedPath = "/Home/Index";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

// ====================================
// JWT AUTHENTICATION CONFIGURATION (Setup & Validation Key)
// ====================================

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.Secret))
    throw new InvalidOperationException("JWT Secret not configured properly in appsettings.json.");

var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

// ====================================
// MULTI-AUTH CONFIGURATION (IDENTITY + GOOGLE + FACEBOOK + JWT)
// ====================================

// Do NOT override Identity's default cookie scheme (which is registered by AddIdentity).
// AddAuthentication is used to add *additional* schemes (JWT, Google, Facebook).
builder.Services.AddAuthentication()
    // JWT Bearer (for API calls)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
        };
    })
    // Google Login
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    // Facebook Login
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    });

// ====================================
// EMAIL + CORS CONFIGURATION
// ====================================

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendAccess", policy =>
    {
        policy.WithOrigins("http://localhost:5078")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ====================================
// DATABASE INITIALIZATION (Roles and SuperAdmin)
// ====================================

// Ensure this block is wrapped in 'using' to properly scope services.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var config = services.GetRequiredService<IConfiguration>();

    try
    {
        string[] roleNames = { "Client", "Admin", "SuperAdmin" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine($"✅ Role '{roleName}' created.");
            }
        }

        // === SUPERADMIN CREATION ===
        var superAdminEmail = config["SuperAdmin:Email"];
        var superAdminPassword = config["SuperAdmin:Password"];
        var superAdminFullName = config["SuperAdmin:FullName"];
        // Removed unused config keys (Gender, Address) from logic for cleaner code
        
        if (!string.IsNullOrWhiteSpace(superAdminEmail))
        {
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdminUser == null)
            {
                var newSuperAdmin = new ApplicationUser(
                    fullName: superAdminFullName ?? "Default SuperAdmin"
                )
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    DateJoined = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                var creationPassword = superAdminPassword ?? "StrongP@ssword123";

                var result = await userManager.CreateAsync(newSuperAdmin, creationPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newSuperAdmin, "SuperAdmin");
                    Console.WriteLine($"✅ SuperAdmin '{superAdminEmail}' created with password set.");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create SuperAdmin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"ℹ️ SuperAdmin '{superAdminEmail}' already exists.");
            }
        }
        else
        {
            Console.WriteLine("⚠️ SuperAdmin email not set in appsettings.json. Skipping SuperAdmin creation.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during DB initialization: {ex.Message}");
    }
}

// ====================================
// MIDDLEWARE CONFIGURATION
// ====================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowFrontendAccess");
app.UseAuthentication();
app.UseAuthorization();

// ====================================
// ROLE-BASED DASHBOARD REDIRECTS (from root '/')
// ====================================

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true && context.Request.Path == "/")
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);

        if (user != null)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (roles.Contains("SuperAdmin"))
            {
                context.Response.Redirect("/Admin/Dashboard");
                return;
            }
            
            if (roles.Contains("Admin"))
            {
                context.Response.Redirect("/Admin/Dashboard");
                return;
            }
            // All other authenticated users (like 'Client') will fall through to the default route.
        }
    }

    await next();
});

// ====================================
// ROUTING CONFIGURATION
// ====================================

app.MapControllers();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();