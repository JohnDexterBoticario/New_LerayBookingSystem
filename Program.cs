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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// EMAIL SERVICES — CLEAN + CORRECT
// ==========================================================
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IEmailSender, EmailService>();

// ==========================================================
// URL HELPERS (NEEDED FOR RESET PASSWORD LINKS)
// ==========================================================
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddScoped<IUrlHelper>(sp =>
{
    var actionContext = sp.GetRequiredService<IActionContextAccessor>().ActionContext
                       ?? new ActionContext();
    return sp.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
});

// ==========================================================
// MVC + RAZOR
// ==========================================================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ==========================================================
// DATABASE
// ==========================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// ==========================================================
// IDENTITY CONFIG
// ==========================================================
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

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Index";
    options.AccessDeniedPath = "/Home/Index";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
});

// ==========================================================
// JWT CONFIG
// ==========================================================
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ==========================================================
// AUTHENTICATION: IDENTITY + GOOGLE + FACEBOOK + JWT
// ==========================================================
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    })
    .AddGoogle(google =>
    {
        google.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        google.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddFacebook(facebook =>
    {
        facebook.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        facebook.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    });

// ==========================================================
// CORS — for your frontend
// ==========================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendAccess", policy =>
    {
        policy.WithOrigins("http://localhost:5078")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ==========================================================
// DB INITIALIZATION — Create roles + SuperAdmin
// ==========================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var config = services.GetRequiredService<IConfiguration>();

    string[] roles = { "Client", "Admin", "SuperAdmin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // SUPERADMIN SETUP
    var email = config["SuperAdmin:Email"];
    var password = config["SuperAdmin:Password"];
    var name = config["SuperAdmin:FullName"];

    if (!string.IsNullOrWhiteSpace(email))
    {
        var superAdmin = await userManager.FindByEmailAsync(email);
        if (superAdmin == null)
        {
            superAdmin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                DateJoined = DateTime.UtcNow
            };

            var create = await userManager.CreateAsync(superAdmin, password);
            if (create.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }
    }
}

// ==========================================================
// MIDDLEWARE
// ==========================================================
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

// ==========================================================
// REDIRECT USER BY ROLE
// ==========================================================
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true &&
        context.Request.Path == "/")
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
        }
    }

    await next();
});

// ==========================================================
// ROUTES
// ==========================================================
app.MapControllers();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
