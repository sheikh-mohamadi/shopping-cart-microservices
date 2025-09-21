using Billing.Service;
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
        services.AddKafkaConsumer(context.Configuration, "billing-service-group");
        services.AddHostedService<Worker>();

        var otlpEndpoint = context.Configuration.GetValue<string>("Otlp:Endpoint");
        var serviceName = context.Configuration.GetValue<string>("Service:Name", "Billing.Service");
        var serviceVersion = context.Configuration.GetValue<string>("Service:Version", "1.0.0");

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName, serviceVersion))
            .WithTracing(tp => tp
                .AddSource(serviceName)
                .AddOtlpExporter(opt =>
                {
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        opt.Endpoint = new Uri(otlpEndpoint);
                }))
            .WithMetrics(mp => mp
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        opt.Endpoint = new Uri(otlpEndpoint);
                }));
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();

        var otlpEndpoint = context.Configuration.GetValue<string>("Otlp:Endpoint");
        var serviceName = context.Configuration.GetValue<string>("Service:Name", "Billing.Service");

        logging.AddOpenTelemetry(opt =>
        {
            opt.IncludeScopes = true;
            opt.IncludeFormattedMessage = true;
            opt.ParseStateValues = true;
            opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            opt.AddOtlpExporter(exporter =>
            {
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    exporter.Endpoint = new Uri(otlpEndpoint);
            });
        });
    })
    .Build();

var kafkaTopics = host.Services
    .GetRequiredService<IConfiguration>()
    .GetSection("Kafka:Topics")
    .Get<string[]>() ?? ["cart-events"];

await host.Services.EnsureKafkaTopicsAsync(kafkaTopics);

await host.RunAsync();