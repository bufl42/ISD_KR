using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Build_BuildersIS.Models
{
    public class MenuItem
    {
        public string Title { get; set; }
        public ICommand Command { get; set; }
    }
}
