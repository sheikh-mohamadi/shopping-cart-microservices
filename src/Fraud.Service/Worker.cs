using System.Text.Json;
using Cart.Domain.Events;
using Confluent.Kafka;
using Fraud.Service.Model;
using Microsoft.ML;

namespace Fraud.Service;

public class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger, MLContext mlContext, ITransformer mlModel)
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Fraud Service starting...");

        using var scope = serviceProvider.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<string, string>>();

        try
        {
            consumer.Subscribe("cart-events");
            logger.LogInformation("Subscribed to cart-events topic");

            // ایجاد PredictionEngine
            var predEngine = mlContext.Model.CreatePredictionEngine<CartData, FraudPrediction>(mlModel);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value == null) continue;

                    logger.LogDebug("Received message for fraud check: {Message}", consumeResult.Message.Value);

                    var cartEvent = JsonSerializer.Deserialize<CartEvent>(
                        consumeResult.Message.Value, JsonOptions);

                    if (cartEvent != null)
                    {
                        await CheckForFraud(cartEvent, predEngine);
                    }

                    consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Error consuming message: {Error}", ex.Error.Reason);
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task CheckForFraud(CartEvent @event, PredictionEngine<CartData, FraudPrediction> predEngine)
    {
        logger.LogInformation(
            "🔍 Checking for fraud in cart {CartId} for user {UserId}",
            @event.CartId, @event.UserId);

        var cartData = new CartData
        {
            ItemCount = @event is ItemAddedEvent ? 1 : 0,
            TotalAmount = @event is ItemAddedEvent added ? (float)added.Item.Price : 0,
            TimeSinceLastEvent = 0
        };

        var prediction = predEngine.Predict(cartData);

        const float fraudThreshold = 0.9f;

        if (prediction.Score > fraudThreshold)
        {
            logger.LogWarning("🚨 Fraud detected in cart {CartId} with score {Score}", @event.CartId, prediction.Score);
        }
        else
        {
            logger.LogInformation("✅ No fraud detected in cart {CartId} with score {Score}", @event.CartId,
                prediction.Score);
        }

        await Task.Delay(150);
    }
}