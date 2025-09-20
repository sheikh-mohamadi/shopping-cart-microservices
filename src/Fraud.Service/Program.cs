using Fraud.Service;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Kernel;
using Shared.Kernel.Kafka;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddKafkaServices(context.Configuration);
        services.AddKafkaConsumer(context.Configuration, "fraud-service-group");
        services.AddHostedService<Worker>();

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("Fraud.Service"))
            .WithTracing(tp => tp
                .AddSource("Fraud.Service")
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(context.Configuration["Otlp:Endpoint"] ?? "http://otel-collector:4317");
                }))
            .WithMetrics(mp => mp
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(context.Configuration["Otlp:Endpoint"] ?? "http://otel-collector:4317");
                }));
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.AddOpenTelemetry(opt =>
        {
            opt.IncludeScopes = true;
            opt.IncludeFormattedMessage = true;
            opt.ParseStateValues = true;
            opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Fraud.Service"));
            opt.AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(context.Configuration["Otlp:Endpoint"] ?? "http://otel-collector:4317");
            });
        });
    })
    .Build();

await host.Services.EnsureKafkaTopicsAsync(["cart-events"]);

await host.RunAsync();