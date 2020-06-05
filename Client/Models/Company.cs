using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFIManager.Models
{
    public class Company
    {
        private string _name;
        public string DisplayName
        {
            get => _name;
            set
            {
                _name = ToTitleCase(value);
            }
        }
        public string DirectoryPath { get; set; }

        public Company(string path)
        {
            DirectoryPath = path;

            DisplayName = new DirectoryInfo(path).Name;
        }

        private string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }
    }
}
