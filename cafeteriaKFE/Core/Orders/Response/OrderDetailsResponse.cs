namespace cafeteriaKFE.Core.Orders.Response
{
    public class OrderDetailCreateResponse
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? SizeId { get; set; }
        public int? MilkTypeId { get; set; }
        public int? TemperatureId { get; set; }
        public int? SyrupId { get; set; }
        public bool HasWhippedCream { get; set; }
    }
}
