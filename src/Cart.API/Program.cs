using StackExchange.Redis;
using Cart.API.Services;
using Shared.Kernel;
using Shared.Kernel.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddKafkaServices(builder.Configuration);
builder.Services.AddKafkaProducer(builder.Configuration);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    var configuration = ConfigurationOptions.Parse(connectionString);
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped<CartService>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var topicManager = scope.ServiceProvider.GetRequiredService<ITopicManager>();
await topicManager.EnsureTopicsExistAsync(new[] { "cart-events" });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Cart API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();