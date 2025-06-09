// Models/LayCreateViewModel.cs
using System.Collections.Generic;

namespace CutUsage.Models
{
    public class LayCreateViewModel
    {
        public LayMaster Lay { get; set; } = new LayMaster();

        public IEnumerable<Marker> Markers { get; set; } = new List<Marker>();
        public IEnumerable<LayType> LayTypes { get; set; } = new List<LayType>();
        public IEnumerable<LayTable> LayTables { get; set; } = new List<LayTable>();

        
    }
}
