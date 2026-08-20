using AuditLogSample.Data;
using AuditLogSample.Handlers;
using AuditLogSample.Models;
using AuditLogSample.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AuditLogDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=AuditLogSample.db"));

// MediatR - only for Audit events
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Simple DI for services
builder.Services.AddSingleton<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IAuditTargetResolver, AuditTargetResolver>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuditLogDbContext>();
    await SqliteSchemaInitializer.InitializeAsync(dbContext);

    var seedUser = await dbContext.Users.FindAsync("user-001");
    if (seedUser is null && !await dbContext.Users.AnyAsync())
    {
        dbContext.Users.Add(new User
        {
            Id = "user-001",
            Name = "Alice Wong",
            Email = "alice@bank.com",
            MobileNo = "09123456789",
            Limit = 100_000,
            Status = "Approved",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        });

        await dbContext.SaveChangesAsync();
    }
    else if (seedUser is not null && string.IsNullOrWhiteSpace(seedUser.MobileNo))
    {
        seedUser.MobileNo = "09123456789";
        await dbContext.SaveChangesAsync();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.Run();
