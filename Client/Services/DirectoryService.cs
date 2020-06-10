using GFIManager.Models;
using GFIManager.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFIManager.Services
{
    public class DirectoryService
    {
        private readonly IEnumerable<Company> companies;
        public DirectoryService(string rootDir)
        {
            companies = Directory.GetDirectories(rootDir).Select(d => new Company(d));
        }

        public IEnumerable<Company> GetCompaniesWithMissingFiles()
        {
            var requiredFiles = new List<string>
            {
                Settings.Default.BilancaFileName,
                Settings.Default.RDGFileName,
                Settings.Default.DodatniFileName
            };

            Predicate<string[]> containsRequiredFiles = (companyFolder) =>
            {
                return
                    companyFolder.Intersect(requiredFiles).Count() == requiredFiles.Count &&
                    companyFolder.Any(el => el.EndsWith(Settings.Default.OldGfiSuffix));
            };

            return companies
                .Select(c => new { Company = c, Files = GetFileNames(c.DirectoryPath) })
                .Where(c => !containsRequiredFiles(c.Files))
                .Select(c => c.Company);
        }

        public IEnumerable<Company> GetCompaniesWithCreatedGfi()
        {
            var missingFilesCompanies = GetCompaniesWithMissingFiles();

            return companies
                .Except(missingFilesCompanies)
                .Select(c => new { Company = c, Files = GetFileNames(c.DirectoryPath) })
                .Where(c => c.Files.Any(f => f.EndsWith($"{Settings.Default.FinalGfiSuffix}.xls")))
                .Select(c => c.Company);
        }

        private string[] GetFileNames(string directoryPath) =>
            Directory.GetFiles(directoryPath).Select(Path.GetFileName).ToArray();

        public IEnumerable<Company> GetCompaniesWithoutNewGfi()
        {
            var missingFilesCompanies = GetCompaniesWithMissingFiles();
            var companiesWithGfi = GetCompaniesWithCreatedGfi();

            return companies.Except(missingFilesCompanies).Except(companiesWithGfi);
        }
    }
}
