namespace CutUsage.Models
{
    public class LayRollDetailsViewModel
    {
        public LayRollHeader Header { get; set; } = new();
        public List<LayRollDetail> Details { get; set; } = new();
        public List<LayDocketSo> DocketDetails { get; set; } = new List<LayDocketSo>();
    }
}