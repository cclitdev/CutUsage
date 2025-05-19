namespace CutUsage.Models
{
    public class LayRollHeader
    {
        public int LayID { get; set; }
        public int MarkerId { get; set; }
        public string LayTypeName { get; set; } = "";
        public string LayTableName { get; set; } = "";
        public string SO { get; set; } = "";
        public string MarkerName { get; set; } = "";
        public decimal MarkerWidth { get; set; }
        public decimal MarkerLength { get; set; }
        public decimal MarkerUsage { get; set; }
        public string FGStyle { get; set; } = "";
        public string FGColor { get; set; } = "";
    }
}