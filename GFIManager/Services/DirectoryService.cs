using GFIManager.Models;
using GFIManager.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GFIManager.Services
{
    public class DirectoryService
    {
        private readonly string root;
        private readonly IEnumerable<Company> companies;
        private readonly NotesBuildingService notesService;

        public DirectoryService(string rootDir)
        {
            root = rootDir;
            companies = Directory.GetDirectories(rootDir).Select(d => new Company(d));
            notesService = new NotesBuildingService(rootDir);
        }

        public IEnumerable<Company> GetCompaniesWithMissingFiles()
        {
            var requiredFiles = new List<string>
            {
                Settings.Default.BilancaFileName,
                Settings.Default.RDGFileName,
                Settings.Default.DodatniFileName
            };

            var oldGfiSuffixRegex = new Regex(Settings.Default.OldGfiSuffix);
            Predicate<string[]> containsRequiredFiles = (companyFiles) =>
            {
                return
                    companyFiles.Intersect(requiredFiles).Count() == requiredFiles.Count &&
                    companyFiles.Any(el => oldGfiSuffixRegex.IsMatch(el));
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
                .Where(c => c.Files.Any(f => f.EndsWith(Settings.Default.FinalGfiSuffix)))
                .Select(c => c.Company);
        }

        public IEnumerable<Company> GetCompaniesWithoutNewGfi()
        {
            var missingFilesCompanies = GetCompaniesWithMissingFiles();
            var companiesWithGfi = GetCompaniesWithCreatedGfi();

            return companies.Except(missingFilesCompanies).Except(companiesWithGfi);
        }

        private string[] GetFileNames(string directoryPath) =>
            Directory.GetFiles(directoryPath).Select(Path.GetFileName).ToArray();

        public Task<IEnumerable<Company>> GetCompaniesWithInvalidGfi()
        {
            return Task.Run(() => GetCompaniesWithCreatedGfi().Where(notesService.CompanyHasInvalidGfi));
        }

        public Task<IEnumerable<Company>> GetCompaniesWithCreatedNotes()
        {
            return notesService.GetCompaniesWithCreatedNotes(companies);
        }

        public async Task<IEnumerable<Company>> GetCompaniesWithoutNotes()
        {
            var invalidGfisTask = GetCompaniesWithInvalidGfi();
            var createdNotesTask = GetCompaniesWithCreatedNotes();
            return GetCompaniesWithCreatedGfi().Except(await invalidGfisTask).Except(await createdNotesTask);
        }
    }
}