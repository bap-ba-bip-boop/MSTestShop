using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSTestShop.Settings;
using MvcSuperShop.Data;
using MvcSuperShop.Infrastructure.Profiles;
using MvcSuperShop.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<HomeControllerSettings>(
    builder.Configuration.GetSection(nameof(HomeControllerSettings))
    );

builder.Services.AddControllersWithViews();

builder.Services.AddTransient<DataInitializer>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IPricingService, PricingService>();
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataInitializer = scope.ServiceProvider.GetService<DataInitializer>();
    dataInitializer!.SeedData();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
