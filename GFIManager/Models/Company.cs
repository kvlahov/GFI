using System.Globalization;
using System.IO;

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

        public Company(string dirPath)
        {
            DirectoryPath = dirPath;

            DisplayName = new DirectoryInfo(dirPath).Name;
        }

        private string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public override bool Equals(object obj) =>
            obj is Company other && DisplayName.Equals(other.DisplayName);

        public override int GetHashCode() => DisplayName.GetHashCode();
    }
}