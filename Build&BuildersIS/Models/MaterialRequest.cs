﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build_BuildersIS.Models
{
    public class MaterialRequest
    {
        public int RequestID { get; set; }
        public int ProjectID { get; set; }
        public string ObjectAddress { get; set; }
        public DateTime RequestDate { get; set; }
        public byte[] ProjectImage { get; set; }
        public List<MaterialItem> Materials { get; set; }
        public string Status { get; set; }

        public string MaterialsSummary
        {
            get
            {
                return string.Join(", ", Materials.Select(m => $"{m.Name} - {m.Quantity} {m.Unit}"));
            }
        }
    }
}
