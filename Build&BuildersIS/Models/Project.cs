using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build_BuildersIS.Models
{
    public class Project
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public byte[] ProjectImage { get; set; }

        // Коллекция объектов, связанных с проектом
        public ObservableCollection<ObjectItem> ProjectObjects { get; set; } = new ObservableCollection<ObjectItem>();
    }
}
