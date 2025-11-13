namespace PsP.Models;

public class PaymentRecord
{
    public int Id { get; set; }
    public string StripeSessionId { get; set; } = null!;
    public DateTime Created { get; set; }
    public string Status { get; set; } = null!;
}