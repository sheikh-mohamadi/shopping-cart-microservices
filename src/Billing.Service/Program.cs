using Billing.Service;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Shared.Kernel;
using Shared.Kernel.Kafka;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
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
                .AddOtlpExporter(opt =>
                    opt.Endpoint = new Uri(context.Configuration["Otlp:Endpoint"] ?? "http://otel-collector:4317")))
            .WithMetrics(mp => mp
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(opt =>
                    opt.Endpoint = new Uri(context.Configuration["Otlp:Endpoint"] ?? "http://otel-collector:4317")));
    })
    .Build();

await host.Services.EnsureKafkaTopicsAsync(["cart-events"]);

await host.RunAsync();