namespace CutUsage.Models
{
    public class DocketDetail
    {
        public string DocketNo { get; set; }
        public string SO { get; set; }
        public string PrdOrder { get; set; }
        public string FGStyle { get; set; }
        public string FGColor { get; set; }
        public string SGStyle { get; set; }
        public string SGColor { get; set; }
        public decimal Qty { get; set; }
        public decimal OddBundleQty { get; set; }
        public decimal Shade { get; set; }
        public string Role { get; set; }
        public decimal BOMUsage { get; set; }
        public string MarkerName { get; set; }
        public decimal MarkerWidth { get; set; }
        public decimal MarkerUsage { get; set; }
        public int NoOfPlys { get; set; }
        public string CustomerStyle { get; set; }
        public decimal SpecWidth { get; set; }
        // Add other properties if needed.
    }
}
