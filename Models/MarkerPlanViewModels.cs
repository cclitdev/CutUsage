using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CutUsage.Models
{
    public class MarkerPlanDetailViewModel
    {
        public string DocketNo { get; set; } = "";
        public string Size { get; set; } = "";
        public decimal Qty { get; set; }
        public decimal ExistingCutQty { get; set; }
        public string MaterialCode { get; set; } = "";
        public decimal BOMUsage { get; set; }
        public int NoOfPlies { get; set; }
        public decimal FabricRequirement { get; set; }
        public decimal MarkerUsage { get; set; }
        public decimal MarkerSaving { get; set; }
        public decimal TargetLength { get; set; }

        // ↓↓↓ new fields ↓↓↓
        public string MarkerName { get; set; } = "";
        public decimal MarkerLength { get; set; }
        public decimal MarkerWidth { get; set; }
        public decimal Allowance { get; set; }
    }

    public class MarkerPlanCreateViewModel
    {
        [Required] public string Style { get; set; } = "";
        [Required, MinLength(1)] public string[] SelectedSO { get; set; } = Array.Empty<string>();
        [Required, MinLength(1)] public string[] SelectedDocket { get; set; } = Array.Empty<string>();

        // don't require Details until you actually bind it
        public List<MarkerPlanDetailViewModel> Details { get; set; }
            = new List<MarkerPlanDetailViewModel>();
    }
}
