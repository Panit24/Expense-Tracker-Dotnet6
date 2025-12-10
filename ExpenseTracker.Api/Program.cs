using ExpenseTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
// Fix PostgreSQL DateTime issue
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger (for testing via browser if you want)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations automatically in dev (optional but handy)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var urls = app.Urls;
Console.WriteLine("========================================");
Console.WriteLine("🚀 Expense Tracker API is running!");
Console.WriteLine("========================================");
foreach (var url in urls)
{
    Console.WriteLine($"📡 Listening on: {url}");
}
Console.WriteLine($"📚 Swagger UI: {urls.FirstOrDefault()}/swagger");
Console.WriteLine("========================================");

app.Run();
