using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build_BuildersIS.Models
{
    public class MaterialItem
    {
        public int MaterialID { get; set; }
        public string Name { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public byte[] ImageData { get; set; }

        public List<MaterialItem> Material { get; set; }

        public string MaterialsSummary
        {
            get
            {
                return string.Join(" ", Material.Select(m => $"{m.Quantity} {m.Unit}"));
            }
        }
    }
}
