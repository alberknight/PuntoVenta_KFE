using cafeteriaKFE.Core.DTOs;

namespace cafeteriaKFE.Core.Pos.Response
{
    public sealed class PosOptionsResponse
    {
        public List<OptionDto> Sizes { get; set; } = [];
        public List<OptionDto> MilkTypes { get; set; } = [];
    }
}
