namespace cafeteriaKFE.Core.Pos.Request
{
    public sealed class AddLineRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;

        public int SizeId { get; set; }
        public int MilkTypeId { get; set; }
    }
}
