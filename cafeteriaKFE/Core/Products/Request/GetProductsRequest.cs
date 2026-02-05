namespace cafeteriaKFE.Core.Products.Request
{
    public class GetProductsRequest
    {
        public int ProductId { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int BarCode { get; set; }
        public decimal BasePrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
