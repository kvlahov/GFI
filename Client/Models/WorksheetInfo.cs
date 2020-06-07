using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFIManager.Models
{
    public class WorksheetInfo
    {
        public string Range { get; set; }
        public string FileName { get; set; }
        public List<string> LockedAops { get; set; }
    }
}
