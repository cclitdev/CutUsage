namespace CutUsage.Models
{
    public class LayMaster
    {
        public int LayID { get; set; }
        public string MarkerId { get; set; }
        public string Style { get; set; }
        public int LayType { get; set; }
        public int LayTable { get; set; }
        public DateTime? LayDate { get; set; }
        public DateTime? LayStartTime { get; set; }
        public DateTime? LayCompleteTime { get; set; }

        // ← NEW:
        public string MarkerName { get; set; } = "";
        public string LayTypeName { get; set; } = "";
        public string LayTableName { get; set; } = "";
    }


    public class LayDetail
    {
        public int LayID { get; set; }
        public string SO { get; set; }
        public string DocketNo { get; set; }
        public string MaterialCode { get; set; }
    }

    public class LayType { public int LayTypeId; public string LayTYpeName; }
    public class LayTable { public int LayTableId; public string LayTableName; }
    public class DocketLookup { public string DocketNo; public string SO; public string MaterialCode; }
}
