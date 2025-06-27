using System.Collections.Generic;

namespace CutUsage.Models
{
    public class MarkerPlanMatrixViewModel
    {
        public List<string> Sizes { get; set; }
        public Dictionary<string, decimal> QtyMap { get; set; }
        public decimal TotalQty { get; set; }
        public Dictionary<string, decimal> ExistingCutMap { get; set; }
        public decimal ExistingCutTotal { get; set; }
        public List<string> Dockets { get; set; }

        // New maps for Material Code and BOM Usage per docket
        public Dictionary<string, string> MaterialCodeMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, decimal> BOMUsageMap { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// [docket] → [ size → ratio ]
        /// </summary>
        public Dictionary<string, Dictionary<string, int>> RatioMap { get; set; }
            = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
    }
}
