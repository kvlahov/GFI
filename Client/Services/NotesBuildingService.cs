using GFIManager.Models;
using GFIManager.Properties;
using Microsoft.Office.Interop.Excel;
using NPOI.HSSF.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFIManager.Services
{
    public class NotesBuildingService : ExcelBaseService
    {
        private readonly string root;
        public NotesBuildingService(string rootDir)
        {
            root = rootDir;
            CreateFileIfNotExists();
        }

        private void CreateFileIfNotExists()
        {
            var path = Path.Combine(root, Settings.Default.BiljeskeFileName);
            if (!File.Exists(path)) File.Create(path).Dispose();
        }

        public bool CompanyHasInvalidGfi(Company company)
        {
            var sw = Stopwatch.StartNew();
            var filePaths = Directory.GetFiles(company.DirectoryPath);
            var gfiFile = filePaths.First(p => p.EndsWith(Settings.Default.FinalGfiSuffix));

            HSSFWorkbook workbook;
            using (FileStream file = new FileStream(gfiFile, FileMode.Open, FileAccess.Read))
            {
                workbook = new HSSFWorkbook(file);
            }

            var sheet = workbook.GetSheet("RefStr");
            var cr = new CellReference("A78");
            var res = sheet.GetRow(cr.Row).GetCell(cr.Col).StringCellValue;

            sw.Stop();

            Debug.WriteLine($"CompanyHasInvalidGfiFaster: {sw.ElapsedMilliseconds/1000f}s");

            return !string.IsNullOrEmpty(res);

        }

        public async Task<IEnumerable<Company>> GetCompaniesWithCreatedNotes(IEnumerable<Company> companies)
        {
            var notesFilePath = Directory
                .GetFiles(root)
                .First(f => f.EndsWith(Settings.Default.BiljeskeFileName));

            var companiesWithNotes = await Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                HSSFWorkbook workbook;
                using (FileStream file = new FileStream(notesFilePath, FileMode.Open, FileAccess.Read))
                {
                    workbook = new HSSFWorkbook(file);
                }

                var sheet = workbook.GetSheetAt(0);

                var companyNamesWithNotes = Enumerable.Range(0, sheet.LastRowNum + 1)
                    .Select(row => sheet.GetRow(row).GetCell(0)?.StringCellValue)
                    .Where(s => !string.IsNullOrWhiteSpace(s));

                sw.Stop();
                Debug.WriteLine($"GetCompaniesWithCreatedNotes: {sw.ElapsedMilliseconds / 1000}");

                return companyNamesWithNotes;
            });

            return companies.Where(c => companiesWithNotes.Contains(c.DisplayName));
        }

        public Task AddNotesForCompanies(IEnumerable<Company> notesToAdd)
        {
            throw new NotImplementedException();
        }

        public Task UpdateNotesForCompanies(IEnumerable<Company> notesToOverride)
        {
            throw new NotImplementedException();
        }
    }
}
