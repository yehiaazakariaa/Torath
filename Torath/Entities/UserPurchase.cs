namespace Torath.Entities
{
    public class UserPurchase
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int NewspaperId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string StripeSessionId { get; set; } = string.Empty;
        public bool IsPaymentComplete { get; set; }
    }
}