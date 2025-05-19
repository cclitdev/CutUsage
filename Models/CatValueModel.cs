namespace CutUsage.Models
{
    public class CatValueModel
    {
        public string DocketNo { get; set; }
        public string SystemBatch { get; set; }  // New property
        public decimal? Cat1Value { get; set; }
        public decimal? Cat2Value { get; set; }
        public decimal? Cat3Value { get; set; }
        public decimal? Cat4Value { get; set; }
    }
}
