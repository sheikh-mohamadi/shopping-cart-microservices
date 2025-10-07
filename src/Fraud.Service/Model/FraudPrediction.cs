using Microsoft.ML.Data;

namespace Fraud.Service.Model;

public class FraudPrediction
{
    [ColumnName("Score")] public float Score { get; set; }
}