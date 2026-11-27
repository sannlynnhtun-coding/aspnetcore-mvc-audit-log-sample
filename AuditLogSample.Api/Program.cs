using System.Text.Json.Serialization;
using AuditLogSample.Api;
using AuditLogSample.Database.AppDbContextModels;
using AuditLogSample.Domain;
using AuditLogSample.Shared;
using Scalar.AspNetCore;    

var builder = WebApplication.CreateBuilder(args);

// Resolve the existing root-level AuditLogSample.db seamlessly whether started from solution root or AuditLogSample.Api/
var rootDbCandidate = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "AuditLogSample.db"));
var localDbCandidate = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "AuditLogSample.db"));
var resolvedDbPath = File.Exists(rootDbCandidate) || !File.Exists(localDbCandidate)
    ? rootDbCandidate
    : localDbCandidate;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddOpenApi();

builder.AddDomain($"Data Source={resolvedDbPath}");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(dbContext);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
