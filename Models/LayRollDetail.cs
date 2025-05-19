namespace CutUsage.Models
{
    public class LayRollDetail
    {
        public string MaterialCode { get; set; } = "";
        public string VendorCode { get; set; } = "";
        public string VendorBatch { get; set; } = "";
        public string SAPBatchNo { get; set; } = "";
        public string RollNo { get; set; } = "";
        public string Shade { get; set; } = "";
        public string MaterialDescription { get; set; } = "";
        public decimal Length { get; set; }
        public decimal? NoOfPlys { get; set; }
        public decimal? PartPly { get; set; }
        public decimal? BindingQty { get; set; }
        public decimal? Cat1Value { get; set; }
        public decimal? Cat2Value { get; set; }
        public decimal? Cat3Value { get; set; }
        public decimal? Cat4Value { get; set; }
    }
}
