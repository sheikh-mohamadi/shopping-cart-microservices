using Fraud.Service.Model;
using Microsoft.ML;

var mlContext = new MLContext(seed: 0);

var data = GenerateSyntheticData(1000);

var dataView = mlContext.Data.LoadFromEnumerable(data);

var pipeline = mlContext.Transforms.Concatenate("Features",
        nameof(CartData.ItemCount),
        nameof(CartData.TotalAmount),
        nameof(CartData.TimeSinceLastEvent))
    .Append(mlContext.AnomalyDetection.Trainers.RandomizedPca(
        rank: 3,
        seed: 0));

var model = pipeline.Fit(dataView);

using var stream = new FileStream("fraudModel.zip", FileMode.Create, FileAccess.Write);
mlContext.Model.Save(model, dataView.Schema, stream);
Console.WriteLine("مدل در fraudModel.zip ذخیره شد.");

var predEngine = mlContext.Model.CreatePredictionEngine<CartData, FraudPrediction>(model);
var testData = new CartData { ItemCount = 50, TotalAmount = 10000, TimeSinceLastEvent = 10 };
var prediction = predEngine.Predict(testData);
Console.WriteLine($"امتیاز ناهنجاری برای داده تستی: {prediction.Score}");

static IEnumerable<CartData> GenerateSyntheticData(int count)
{
    var random = new Random(0);
    var data = new List<CartData>();

    for (int i = 0; i < count; i++)
    {
        data.Add(new CartData
        {
            ItemCount = random.Next(1, 10),
            TotalAmount = random.Next(10, 1000),
            TimeSinceLastEvent = random.Next(0, 3600)
        });
    }

    data.AddRange([
        new CartData { ItemCount = 50f, TotalAmount = 10000f, TimeSinceLastEvent = 10f },
        new CartData { ItemCount = 1f, TotalAmount = 5000f, TimeSinceLastEvent = 5f }
    ]);

    return data;
}