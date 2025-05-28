using System.Security.Claims;
using kursachPosuda.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.Identity?.IsAuthenticated == true
            && context.User.FindFirst(ClaimTypes.Email)?.Value == "admin@example.com"));
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Убрали true из первого вызова
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
})
.AddRoles<IdentityRole>()
.AddRussianIdentityErrorDescriber()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.Initialize(services, "dmitryshibaev00@outlook.com"); // Ваш email
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();
app.Run();
