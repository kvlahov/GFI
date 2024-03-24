using GFIManager.Models;
using GFIManager.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GFIManager.Services.Notes
{
    public class DirectoryService
    {
        private readonly string root;
        private readonly IEnumerable<Company> companies;

        public DirectoryService(string rootDir)
        {
            root = rootDir;
            companies = Directory.GetDirectories(rootDir).Select(d => new Company(d));
        }

        public IEnumerable<Company> GetCompaniesWithoutNotes()
        {
            var missingFilesCompanies = GetCompaniesWithMissingFiles();
            var companiesWithGfi = GetCompaniesWithCreatedNotes();

            return companies.Except(missingFilesCompanies).Except(companiesWithGfi);
        }

        public IEnumerable<Company> GetCompaniesWithMissingFiles()
        {
            var requiredFiles = new List<string>
            {
                Settings.Default.GFIFileName,
                Settings.Default.OPFileName,
                Settings.Default.PDFileName
            };

            Predicate<string[]> containsRequiredFiles = (companyFiles) =>
            {
                return companyFiles.Intersect(requiredFiles).Count() == requiredFiles.Count;
            };

            return companies
                .Select(c => new { Company = c, Files = GetFileNames(c.DirectoryPath) })
                .Where(c => !containsRequiredFiles(c.Files))
                .Select(c => c.Company);
        }

        public IEnumerable<Company> GetCompaniesWithCreatedNotes()
        {
            return companies
                .Select(c => new { Company = c, Files = GetFileNames(c.DirectoryPath) })
                .Where(c => c.Files.Any(f => f.EndsWith(Settings.Default.NotesFileName)))
                .Select(c => c.Company);
        }

        private string[] GetFileNames(string directoryPath) =>
            Directory.GetFiles(directoryPath).Select(Path.GetFileName).ToArray();


    }
}