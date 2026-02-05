namespace cafeteriaKFE.Core.Pos.Response
{
    public sealed class CheckoutResponse
    {
        public long OrderId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
