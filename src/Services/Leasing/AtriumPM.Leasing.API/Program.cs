using System.Text;
using AtriumPM.Leasing.API.Application.Interfaces;
using AtriumPM.Leasing.API.Application.Services;
using AtriumPM.Leasing.API.Infrastructure.Data;
using AtriumPM.Shared.Data;
using AtriumPM.Shared.Interfaces;
using AtriumPM.Shared.Middleware;
using AtriumPM.Shared.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ITenantConnectionStringResolver, TenantConnectionStringResolver>();
builder.Services.AddScoped<TenantSessionContextConnectionInterceptor>();

builder.Services.AddDbContext<LeasingDbContext>((sp, options) =>
{
    var defaultConnection = builder.Configuration.GetConnectionString("LeasingDb")
        ?? throw new InvalidOperationException("Connection string 'LeasingDb' is not configured.");

    var connectionResolver = sp.GetRequiredService<ITenantConnectionStringResolver>();
    var resolvedConnection = connectionResolver.ResolveConnectionString(defaultConnection);

    options.UseSqlServer(resolvedConnection);
    options.AddInterceptors(sp.GetRequiredService<TenantSessionContextConnectionInterceptor>());
});
builder.Services.AddScoped<ILeaseService, LeaseService>();
builder.Services.AddScoped<IOccupancyReportService, OccupancyReportService>();

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();

    cfg.UsingRabbitMq((context, rabbitCfg) =>
    {
        rabbitCfg.Host(builder.Configuration.GetConnectionString("RabbitMq") ?? "rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        rabbitCfg.ConfigureEndpoints(context);
    });
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
        Title = "AtriumPM Leasing API",
        Version = "v1",
        Description = "Multi-tenant Leasing service for lease lifecycle and occupancy reporting."
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AtriumPM Leasing API v1");
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
    var db = scope.ServiceProvider.GetRequiredService<LeasingDbContext>();
    await db.Database.MigrateAsync();
    await db.EnsureTenantRlsPoliciesAsync();
}

app.Run();
