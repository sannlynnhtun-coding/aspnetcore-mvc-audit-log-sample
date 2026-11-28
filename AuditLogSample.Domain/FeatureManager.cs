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

        // MediatR - for publishing and handling AuditLogEventRequest
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(FeatureManager).Assembly));

        // AuditLog feature
        builder.Services.AddScoped<IAuditLogDbService, AuditLogDbService>();
        builder.Services.AddScoped<IAuditService, AuditLogService>();

        // Users feature
        builder.Services.AddScoped<UserValidationPolicy>();
        builder.Services.AddScoped<IUserService, UserService>();

    }
}
