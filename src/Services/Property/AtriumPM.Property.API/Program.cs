using System.Text;
using AtriumPM.Property.API.Application.Interfaces;
using AtriumPM.Property.API.Application.Services;
using AtriumPM.Property.API.Infrastructure.Data;
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

builder.Services.AddDbContext<PropertyDbContext>((sp, options) =>
{
    var defaultConnection = builder.Configuration.GetConnectionString("PropertyDb")
        ?? throw new InvalidOperationException("Connection string 'PropertyDb' is not configured.");

    var connectionResolver = sp.GetRequiredService<ITenantConnectionStringResolver>();
    var resolvedConnection = connectionResolver.ResolveConnectionString(defaultConnection);

    options.UseSqlServer(resolvedConnection);
    options.AddInterceptors(sp.GetRequiredService<TenantSessionContextConnectionInterceptor>());
});

builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IUnitService, UnitService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "AtriumPM:Property:";
});

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
        Title = "AtriumPM Property API",
        Version = "v1",
        Description = "Multi-tenant Property service for Buildings and Units."
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AtriumPM Property API v1");
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
    var db = scope.ServiceProvider.GetRequiredService<PropertyDbContext>();
    await db.Database.MigrateAsync();
    await db.EnsureTenantRlsPoliciesAsync();
}

app.Run();
