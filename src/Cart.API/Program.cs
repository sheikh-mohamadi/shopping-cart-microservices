using Cart.API.Data;
using Cart.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Kernel;
using Shared.Kernel.Kafka;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var otlpEndpoint = builder.Configuration.GetValue<string>("Otlp:Endpoint")
                   ?? throw new InvalidOperationException("OTLP endpoint is not configured");

var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString")
                            ?? throw new InvalidOperationException("Redis connection string is not configured");

var postgresConnectionString = builder.Configuration.GetValue<string>("ConnectionStrings:EventStore")
                               ?? throw new InvalidOperationException("Postgres connection string is not configured");

builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(opt =>
{
    opt.IncludeScopes = true;
    opt.IncludeFormattedMessage = true;
    opt.ParseStateValues = true;
    opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Cart.API"));
    opt.AddOtlpExporter(exporter =>
    {
        exporter.Endpoint = new Uri(otlpEndpoint);
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddKafkaServices(builder.Configuration);
builder.Services.AddKafkaProducer(builder.Configuration);

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddDbContext<CartDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

builder.Services.AddScoped<CartService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("Cart.API"))
    .WithTracing(tp => tp
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Cart.API")
        .AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri(otlpEndpoint);
        }))
    .WithMetrics(mp => mp
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri(otlpEndpoint);
        }));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var topicManager = scope.ServiceProvider.GetRequiredService<ITopicManager>();
    await topicManager.EnsureTopicsExistAsync(["cart-events"]);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();