namespace Fraud.Service.Model;

public class CartData
{
    public float ItemCount { get; set; }
    public float TotalAmount { get; set; }
    public float TimeSinceLastEvent { get; set; }
}