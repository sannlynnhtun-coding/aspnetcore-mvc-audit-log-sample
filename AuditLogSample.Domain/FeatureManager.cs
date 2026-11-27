using AuditLogSample.Database.AppDbContextModels;
using AuditLogSample.Domain.Features.AuditLog;
using AuditLogSample.Domain.Features.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace AuditLogSample.Domain;

public static class FeatureManager
{
    public static void AddDomain(
        this WebApplicationBuilder builder,
        string? fallbackConnectionString = null)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? fallbackConnectionString
            ?? "Data Source=AuditLogSample.db";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // MediatR - for publishing and handling AuditEvent
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(FeatureManager).Assembly));

        // AuditLog feature
        builder.Services.AddScoped<IAuditTargetResolver, AuditTargetResolver>();
        builder.Services.AddScoped<IAuditService, AuditLogService>();

        // Users feature
        builder.Services.AddScoped<UserValidationPolicy>();
        builder.Services.AddScoped<IUserService, UserService>();

    }
}
