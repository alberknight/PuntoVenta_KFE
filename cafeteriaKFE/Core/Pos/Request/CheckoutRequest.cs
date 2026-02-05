namespace cafeteriaKFE.Core.Pos.Request
{
    public sealed class CheckoutRequest
    {
        public bool IsDelivery { get; set; }
        public int PaidMethodId { get; set; }
        public List<AddLineRequest> Lines { get; set; } = [];
    }
}
