using METS.StockReplenishment.Core.Interfaces;
using METS.StockReplenishment.Infrastructure.Data;
using METS.StockReplenishment.Infrastructure.Repositories;
using METS.StockReplenishment.Infrastructure.Services;
using METS.StockReplenishment.Web.Components;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// SQLite in-memory: keep a single connection open for the app lifetime
var connection = new SqliteConnection("DataSource=METSStockReplenishment;Mode=Memory;Cache=Shared");
connection.Open();
builder.Services.AddSingleton(connection);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connection));

// Repositories & Services
builder.Services.AddScoped<IReplenishmentRequestRepository, ReplenishmentRequestRepository>();
builder.Services.AddScoped<IReplenishmentRequestService, ReplenishmentRequestService>();
builder.Services.AddScoped<IStockValidationService, StockValidationService>();

// Background service for async stock validation
builder.Services.AddSingleton<StockValidationBackgroundService>();
builder.Services.AddSingleton<IStockValidationQueue>(sp => sp.GetRequiredService<StockValidationBackgroundService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<StockValidationBackgroundService>());

// API Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// MudBlazor
builder.Services.AddMudServices();

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient for Blazor components to call the local API
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

var app = builder.Build();

// Create database schema and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    DataSeeder.Seed(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
