using System.Text;
using AtriumPM.Billing.API.Application.Interfaces;
using AtriumPM.Billing.API.Application.Services;
using AtriumPM.Billing.API.Infrastructure.Consumers;
using AtriumPM.Billing.API.Infrastructure.Data;
using AtriumPM.Billing.API.Infrastructure.Jobs;
using AtriumPM.Shared.Interfaces;
using AtriumPM.Shared.Middleware;
using AtriumPM.Shared.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BillingDbContext>((_, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("BillingDb"));
});

builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<ILateFeeService, LateFeeService>();

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<LeaseSignedConsumer>();
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

builder.Services.AddQuartz(cfg =>
{
    var jobKey = new JobKey("nightly-late-fee-job");

    cfg.AddJob<NightlyLateFeeJob>(opts => opts.WithIdentity(jobKey));
    cfg.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("nightly-late-fee-job-trigger")
        .WithCronSchedule("0 0 1 * * ?"));
});
builder.Services.AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

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
        Title = "AtriumPM Billing API",
        Version = "v1",
        Description = "Multi-tenant Billing service for invoices, payments and nightly late fees."
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AtriumPM Billing API v1");
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
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
