using System.Text;
using AtriumPM.Maintenance.API.Application.Interfaces;
using AtriumPM.Maintenance.API.Application.Services;
using AtriumPM.Maintenance.API.Infrastructure.Data;
using AtriumPM.Shared.Data;
using AtriumPM.Shared.Interfaces;
using AtriumPM.Shared.Middleware;
using AtriumPM.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ITenantConnectionStringResolver, TenantConnectionStringResolver>();
builder.Services.AddScoped<TenantSessionContextConnectionInterceptor>();

builder.Services.AddDbContext<MaintenanceDbContext>((sp, options) =>
{
    var defaultConnection = builder.Configuration.GetConnectionString("MaintenanceDb")
        ?? throw new InvalidOperationException("Connection string 'MaintenanceDb' is not configured.");

    var connectionResolver = sp.GetRequiredService<ITenantConnectionStringResolver>();
    var resolvedConnection = connectionResolver.ResolveConnectionString(defaultConnection);

    options.UseSqlServer(resolvedConnection);
    options.AddInterceptors(sp.GetRequiredService<TenantSessionContextConnectionInterceptor>());
});
builder.Services.AddScoped<IMaintenanceTicketService, MaintenanceTicketService>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "AtriumPM Maintenance API",
        Version = "v1",
        Description = "Multi-tenant Maintenance service for tickets and work orders."
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AtriumPM Maintenance API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>();
    await db.Database.MigrateAsync();
    await db.EnsureTenantRlsPoliciesAsync();
}

app.Run();
