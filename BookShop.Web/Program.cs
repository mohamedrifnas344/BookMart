using BookShop.Application.Interfaces;
using BookShop.Application.IServices;
using BookShop.Domain.Entities;
using BookShop.Infrastructure.Data;
using BookShop.Infrastructure.Repositories;
using BookShop.Infrastructure.Services.Auth;
using BookShop.Infrastructure.Services.CloudinaryService;
using BookShop.Infrastructure.Services.StripePayment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryImage, CloudinaryImage>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

builder.Services.AddIdentity<AppUser, IdentityRole>(option =>
{
    option.Password.RequireNonAlphanumeric = false;
    option.Password.RequireLowercase = false;
    option.Password.RequireUppercase = false;
    option.Password.RequiredLength = 6;
    option.Password.RequireDigit = false;
}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = "/Identity/Account/Login";
    option.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    try
    {
        var userManager = service.GetRequiredService<UserManager<AppUser>>();
        var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = service.GetRequiredService<ILoggerFactory>();

        await IdentitySeeder.LoadDataAsync(userManager, roleManager, logger);
    }catch(Exception ex)
    {
        var logger = app.Services.GetRequiredService<Logger<Program>>();
        logger.LogError(ex, "Ann error occured while seeding identity data.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

Stripe.StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings:SecretKey").Get<string>();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
