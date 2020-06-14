using GFIManager.Models;
using GFIManager.Properties;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GFIManager.Services
{
    public class NotesBuildingService
    {
        private readonly string root;
        private readonly string notesFilePath;

        public NotesBuildingService(string rootDir)
        {
            root = rootDir;
            CreateFileIfNotExists();
            notesFilePath = Directory.GetFiles(root).First(f => f.EndsWith(Settings.Default.BiljeskeFileName));
        }

        private void CreateFileIfNotExists()
        {
            var path = Path.Combine(root, Settings.Default.BiljeskeFileName);
            if (!File.Exists(path))
            {
                var workbook = new HSSFWorkbook();
                workbook.CreateSheet();
                //set headers

                using (FileStream outputStream = new FileStream(path, FileMode.Create))
                {
                    workbook.Write(outputStream);
                }
            }
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

            Debug.WriteLine($"CompanyHasInvalidGfiFaster: {sw.ElapsedMilliseconds / 1000f}s");

            return !string.IsNullOrEmpty(res);
        }

        public async Task<IEnumerable<Company>> GetCompaniesWithCreatedNotes(IEnumerable<Company> companies)
        {
            var companiesWithNotes = await Task.Run(() =>
            {
                HSSFWorkbook workbook;
                using (FileStream file = new FileStream(notesFilePath, FileMode.Open, FileAccess.Read))
                {
                    workbook = new HSSFWorkbook(file);
                }

                var sheet = workbook.GetSheetAt(0);

                var companyNamesWithNotes = Enumerable.Range(0, sheet.LastRowNum + 1)
                    .Where(row => sheet.GetRow(row) != null)
                    .Select(row => sheet.GetRow(row).GetCell(0)?.StringCellValue)
                    .Where(s => !string.IsNullOrWhiteSpace(s));

                return companyNamesWithNotes;
            });

            return companies.Where(c => companiesWithNotes.Contains(c.DisplayName));
        }

        public IDictionary<string, List<string>> GetDataForNotes(IEnumerable<Company> companies)
        {
            return companies.AsParallel().Select(c => ProcessSingleCompany(c)).ToDictionary(k => k.Key, v => v.Value);
        }

        public KeyValuePair<string, List<string>> ProcessSingleCompany(Company company)
        {
            var files = Directory.GetFiles(company.DirectoryPath);

            var filePath = files.First(f => f.EndsWith(Settings.Default.FinalGfiSuffix));

            var bilancaValues = ProccessSingleSheet("H9", "J9", filePath, WorkbookType.Bilanca);
            var rdgValues = ProccessSingleSheet("H8", "J8", filePath, WorkbookType.RDG);

            return new KeyValuePair<string, List<string>>(company.DisplayName, bilancaValues.Concat(rdgValues).ToList());
        }

        private List<string> ProccessSingleSheet(string noteStartCell, string valueStartCell, string filePath, WorkbookType sheetName)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var workbook = WorkbookFactory.Create(file);

                var sheet = workbook.GetSheet(sheetName.ToString());

                var noteStart = new CellReference(noteStartCell);
                var valueStart = new CellReference(valueStartCell);

                var noteColumnIndex = noteStart.Col;
                var valueColumnIndex = valueStart.Col;

                var startRow = noteStart.Row;
                var valuesList = new List<string>();

                HSSFFormulaEvaluator formula = new HSSFFormulaEvaluator(workbook);

                for (int i = 0; i < sheet.LastRowNum - startRow; i++)
                {
                    var currentRow = sheet.GetRow(startRow + i);

                    var currentCellValue = currentRow.GetCell(noteColumnIndex)?.StringCellValue;
                    if (string.IsNullOrWhiteSpace(currentCellValue)) continue;

                    var cell = currentRow.GetCell(valueColumnIndex);
                    formula.EvaluateInCell(cell);

                    var value = cell.NumericCellValue.ToString();
                    valuesList.Add(value);
                }

                return valuesList;
            }
        }

        public void AddNotesForCompanies(IDictionary<string, List<string>> notesToAdd)
        {
            using (FileStream file = new FileStream(notesFilePath, FileMode.Open, FileAccess.Read))
            {
                var workbook = WorkbookFactory.Create(file);

                var sheet = workbook.GetSheetAt(0);

                var startingRow = sheet.GetRow(sheet.LastRowNum) == null ? sheet.LastRowNum : sheet.LastRowNum + 1;
                var companiesArray = notesToAdd.ToArray();
                Enumerable.Range(0, notesToAdd.Count())
                    .ToList()
                    .ForEach(i =>
                    {
                        var currentRow = sheet.CreateRow(startingRow + i);
                        currentRow.CreateCell(0).SetCellValue(companiesArray.ElementAt(i).Key);
                        SetCompanyRow(currentRow, companiesArray.ElementAt(i).Value.ToArray());
                    });

                FileStream outputStream = new FileStream(notesFilePath, FileMode.Create);
                workbook.Write(outputStream);
                outputStream.Close();
            }
        }

        public void UpdateNotesForCompanies(IDictionary<string, List<string>> notesToOverride)
        {
            using (FileStream file = new FileStream(notesFilePath, FileMode.Open, FileAccess.Read))
            {
                var workbook = WorkbookFactory.Create(file);

                var sheet = workbook.GetSheetAt(0);

                var companiesProcessed = 0;
                for (int i = 0; i <= sheet.LastRowNum; i++)
                {
                    var currentRow = sheet.GetRow(i);
                    var currentCompanyName = currentRow.GetCell(0).StringCellValue;
                    var result = notesToOverride.TryGetValue(currentCompanyName, out List<string> valuesToUpdate);

                    if (!result) continue;

                    SetCompanyRow(currentRow, valuesToUpdate.ToArray());
                    companiesProcessed++;

                    if (companiesProcessed == notesToOverride.Count()) break;
                }

                FileStream outputStream = new FileStream(notesFilePath, FileMode.Create);
                workbook.Write(outputStream);
                outputStream.Close();
            }
        }

        private void SetCompanyRow(IRow currentRow, params string[] values)
        {
            for (int i = 1; i <= values.Length; i++)
            {
                var cell = currentRow.GetCell(i);
                var value = Convert.ToInt64(values[i - 1]);
                if (cell == null)
                {
                    currentRow.CreateCell(i).SetCellValue(value);
                }
                else
                {
                    cell.SetCellValue(value);
                }
            }
        }
    }
}