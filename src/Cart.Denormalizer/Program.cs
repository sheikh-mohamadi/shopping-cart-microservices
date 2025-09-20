using Cart.Denormalizer;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Kernel;
using Shared.Kernel.Kafka;
using StackExchange.Redis;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddKafkaServices(context.Configuration);
        services.AddKafkaConsumer(context.Configuration, "cart-denormalizer-group");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = context.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
            var configuration = ConfigurationOptions.Parse(connectionString);
            return ConnectionMultiplexer.Connect(configuration);
        });

        services.AddHostedService<Worker>();

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("Cart.Denormalizer"))
            .WithTracing(tp => tp
                .AddSource("Cart.Denormalizer")
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
            opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Cart.Denormalizer"));
            opt.AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(context.Configuration["Otlp:Endpoint"] ?? "http://otel-collector:4317");
            });
        });
    })
    .Build();

await host.Services.EnsureKafkaTopicsAsync(["cart-events"]);

await host.RunAsync();