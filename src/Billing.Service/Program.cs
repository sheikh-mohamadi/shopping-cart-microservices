using Billing.Service;
using Shared.Kernel;
using Shared.Kernel.Kafka;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "shopping-cart-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((context, services) =>
    {
        services.AddKafkaServices(context.Configuration);
        services.AddKafkaConsumer(context.Configuration, "billing-service-group");
        services.AddHostedService<Worker>();

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("Billing.Service"))
            .WithTracing(tp => tp
                .AddSource("Billing.Service")
                .AddOtlpExporter(opt => opt.Endpoint = new Uri(context.Configuration["Otlp:Endpoint"] ?? "http://otel-collector:4317")))
            .WithMetrics(mp => mp
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(opt => opt.Endpoint = new Uri(context.Configuration["Otlp:Endpoint"] ?? "http://otel-collector:4317")));
    })
    .Build();

using var scope = host.Services.CreateScope();
var topicManager = scope.ServiceProvider.GetRequiredService<ITopicManager>();
await topicManager.EnsureTopicsExistAsync(["cart-events"]);

await host.RunAsync();