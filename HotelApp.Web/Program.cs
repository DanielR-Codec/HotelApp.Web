using System.Globalization;
using HotelApp.BL;
using HotelApp.BL.Options;
using HotelApp.DAL;
using HotelApp.DAL.Context;
using HotelApp.Web.BackgroundServices;
using HotelApp.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Every page requires a logged-in user unless it is marked [AllowAnonymous] (RF-011)
    options.Filters.Add(new AuthorizeFilter());

    // Every POST form must carry the anti-forgery token (protection against CSRF)
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// CU-001 / RF-011: cookie-based login
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddDbContext<HotelContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("HotelApp.DAL")));

builder.Services.AddDALDependencies();
builder.Services.AddBLDependencies();

// Presentation helpers
builder.Services.AddScoped<StructureLookupService>();

// CU-015: SMTP settings for the confirmation / cancellation emails (section "Email" of appsettings)
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

// RN-009: automatic No-Show
builder.Services.AddHostedService<NoShowBackgroundService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Dominican Republic culture: dates dd/MM/yyyy and "." as decimal separator (the same one the client-side validation uses)
var culture = new CultureInfo("es-DO");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
