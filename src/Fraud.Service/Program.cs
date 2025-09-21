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

        var serviceName = context.Configuration["Service:Name"] ?? "Fraud.Service";
        var otlpEndpoint = context.Configuration["Otlp:Endpoint"] 
                           ?? "http://otel-collector:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(tp => tp
                .AddSource(serviceName)
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(otlpEndpoint);
                }))
            .WithMetrics(mp => mp
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(otlpEndpoint);
                }));
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();

        var serviceName = context.Configuration["Service:Name"] ?? "Fraud.Service";
        var otlpEndpoint = context.Configuration["Otlp:Endpoint"] 
                           ?? "http://otel-collector:4317";

        logging.AddOpenTelemetry(opt =>
        {
            opt.IncludeScopes = true;
            opt.IncludeFormattedMessage = true;
            opt.ParseStateValues = true;
            opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            opt.AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(otlpEndpoint);
            });
        });
    })
    .Build();

var topics = host.Services.GetRequiredService<IConfiguration>()
    .GetSection("Kafka:Topics")
    .Get<string[]>() ?? ["cart-events"];

await host.Services.EnsureKafkaTopicsAsync(topics);

await host.RunAsync();