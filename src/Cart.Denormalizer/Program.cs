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
        var configuration = context.Configuration;

        services.AddKafkaServices(configuration);
        services.AddKafkaConsumer(configuration, "cart-denormalizer-group");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration.GetValue<string>("Redis:ConnectionString")
                               ?? throw new InvalidOperationException("Redis connection string is not configured.");
            var options = ConfigurationOptions.Parse(connectionString);
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddHostedService<Worker>();

        var otlpEndpoint = configuration.GetValue<string>("Otlp:Endpoint")
                         ?? throw new InvalidOperationException("OTLP endpoint is not configured.");

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("Cart.Denormalizer"))
            .WithTracing(tp => tp
                .AddSource("Cart.Denormalizer")
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
        var otlpEndpoint = context.Configuration.GetValue<string>("Otlp:Endpoint")
                         ?? throw new InvalidOperationException("OTLP endpoint is not configured.");

        logging.ClearProviders();
        logging.AddOpenTelemetry(opt =>
        {
            opt.IncludeScopes = true;
            opt.IncludeFormattedMessage = true;
            opt.ParseStateValues = true;
            opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Cart.Denormalizer"));
            opt.AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(otlpEndpoint);
            });
        });
    })
    .Build();

await host.Services.EnsureKafkaTopicsAsync(["cart-events"]);

await host.RunAsync();