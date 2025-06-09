using System.ComponentModel.DataAnnotations;

namespace CutUsage.Models
{
    public class Marker
    {
        public string MarkerId { get; set; } = "-1";

        [Required(ErrorMessage = "Marker Name is required")]
        public string MarkerName { get; set; }

        [Required(ErrorMessage = "Marker Width is required")]
        [Range(0, 1000, ErrorMessage = "Width must be a positive number")]
        public decimal MarkerWidth { get; set; }

        [Required(ErrorMessage = "Marker Length is required")]
        [Range(0, 1000, ErrorMessage = "Length must be a positive number")]
        public decimal MarkerLength { get; set; }

        [Required(ErrorMessage = "Marker Usage is required")]
        [Range(0, 1000, ErrorMessage = "Usage must be a positive number")]
        public decimal MarkerUsage { get; set; }
    }
}
