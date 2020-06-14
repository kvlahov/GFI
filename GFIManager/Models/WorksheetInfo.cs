using System.Collections.Generic;

namespace GFIManager.Models
{
    public class WorksheetInfo
    {
        public string Range { get; set; }
        public string FileName { get; set; }
        public List<string> LockedAops { get; set; }
    }
}