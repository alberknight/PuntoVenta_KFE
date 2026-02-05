namespace cafeteriaKFE.Core.DTOs
{
    public sealed class OptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal PriceDelta { get; set; }
    }
}
