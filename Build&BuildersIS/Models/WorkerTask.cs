using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build_BuildersIS.Models
{
    public class WorkerTask
    {
        public int TaskID { get; set; }
        public string ObjectAddress { get; set; }
        public string TaskDescription { get; set; }
        public string TaskStatus { get; set; }
        public DateTime Deadline { get; set; }
        public byte[] ObjectImage { get; set; }
    }
}
