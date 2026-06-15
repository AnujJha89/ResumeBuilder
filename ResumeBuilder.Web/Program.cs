using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ResumeBuilder.Core.Entities;
using ResumeBuilder.Core.Interfaces;
using ResumeBuilder.Infrastructure.Data;
using ResumeBuilder.Infrastructure.Repositories;
using ResumeBuilder.Infrastructure.Services;
using PuppeteerSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString.Contains("Host=") || connectionString.Contains("port=") || connectionString.Contains("sslmode="))
    {
        options.UseNpgsql(connectionString, b => b.MigrationsAssembly("ResumeBuilder.Infrastructure"));
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();

// Register Services
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// Download Chromium for PuppeteerSharp if not already cached
// This runs once at startup so the first PDF request isn't slow.
// Skip if using a pre-installed Chromium path (e.g. in Docker).
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH")))
{
    var browserFetcherTask = Task.Run(async () =>
    {
        var fetcher = new BrowserFetcher();
        await fetcher.DownloadAsync();
    });
}

// Register ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie behaviors
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Configure Google OAuth
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "YOUR_GOOGLE_CLIENT_SECRET";

        // For local development on HTTP (non-SSL), configure correlation and nonce cookies 
        // to not require HTTPS, avoiding "Correlation failed" errors.
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
    });

var app = builder.Build();

// Configure static WebRootPath for profile picture rendering
ResumeBuilder.Infrastructure.Services.PdfService.WebRootPath = app.Environment.WebRootPath;

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database templates.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
