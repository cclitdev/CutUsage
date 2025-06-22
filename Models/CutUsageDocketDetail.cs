namespace CutUsage.Models
{
    public class CutUsageDocketDetail
    {
        public int ID { get; set; }
        public string PlantID { get; set; } = default!;
        public string DocketNo { get; set; } = default!;
        public string CombineOrder { get; set; } = default!;
        public decimal? SpecWidth { get; set; }
        public decimal? ActualWidth { get; set; }
        public decimal? BOMUsage { get; set; }
        public string? MarkerID { get; set; }
        public int? NoOfPlys { get; set; }
    }
}
