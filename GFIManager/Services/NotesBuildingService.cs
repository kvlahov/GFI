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
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "biljeske-template.xls");
                //var templatePath = @"C:\Users\evlakre\source\repos\GFI\GFIManager\Assets\biljeske-template.xls";

                using (FileStream inputStream = new FileStream(templatePath, FileMode.Open))
                {
                    var workbook = new HSSFWorkbook(inputStream);
                    using (FileStream outputStream = new FileStream(path, FileMode.Create))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        public bool CompanyHasInvalidGfi(Company company)
        {
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
                var dataStartRow = 3;
                var companyPathsWithNotes = Enumerable.Range(dataStartRow, sheet.LastRowNum + 1 - (dataStartRow - 1))
                    .Where(row => sheet.GetRow(row) != null)
                    .Select(row => sheet.GetRow(row).GetCell(0)?.StringCellValue)
                    .Where(s => !string.IsNullOrWhiteSpace(s));

                return companyPathsWithNotes;
            });

            return companies.Where(c => companiesWithNotes.Contains(c.DirectoryPath));
        }

        public IDictionary<string, List<string>> GetDataForNotes(IEnumerable<Company> companies)
        {
            var sheetTargetCells = GetSheetsWithTargetCellsFromNotes();
            return companies.AsParallel().Select(c => ProcessSingleCompany(c, sheetTargetCells)).ToDictionary(k => k.Key, v => v.Value);
        }

        private IDictionary<WorkbookType, List<string>> GetSheetsWithTargetCellsFromNotes()
        {
            var refRange = CellRangeAddress.ValueOf("D2:L2");
            var bilRange = CellRangeAddress.ValueOf("N2:BL2");
            var rdgRange = CellRangeAddress.ValueOf("BM2:CC2");

            using (FileStream file = new FileStream(notesFilePath, FileMode.Open, FileAccess.Read))
            {
                var workbook = WorkbookFactory.Create(file);

                var sheet = workbook.GetSheetAt(0);
                var refValues = GetCellValues(sheet, refRange);
                var bilValues = GetCellValues(sheet, bilRange);
                var rdgValues = GetCellValues(sheet, rdgRange);

                return new Dictionary<WorkbookType, List<string>>
                {
                    { WorkbookType.RefStr, refValues},
                    { WorkbookType.Bilanca, bilValues},
                    { WorkbookType.RDG, rdgValues},
                };
            }
        }

        private List<string> GetCellValues(ISheet sheet, CellRangeAddress range)
        {
            return Enumerable.Range(range.FirstColumn, range.LastColumn - range.FirstColumn + 1)
                                 .Select(i => sheet.GetRow(range.FirstRow).GetCell(i).StringCellValue)
                                 .ToList();
        }


        public KeyValuePair<string, List<string>> ProcessSingleCompany(Company company, IDictionary<WorkbookType, List<string>> sheetTargetCells)
        {
            var files = Directory.GetFiles(company.DirectoryPath);
            var filePath = files.First(f => f.EndsWith(Settings.Default.FinalGfiSuffix));

            var refValues = ProccessSingleSheet(sheetTargetCells[WorkbookType.RefStr], filePath, WorkbookType.RefStr);
            var bilancaValues = ProccessSingleSheet(sheetTargetCells[WorkbookType.Bilanca], filePath, WorkbookType.Bilanca);
            var rdgValues = ProccessSingleSheet(sheetTargetCells[WorkbookType.RDG], filePath, WorkbookType.RDG);
            string aktiva = GetCellValueFromSheet(filePath, WorkbookType.Bilanca, "J73");

            return new KeyValuePair<string, List<string>>(company.DirectoryPath, refValues.Concat(bilancaValues).Concat(rdgValues).Append(aktiva).ToList());
        }

        private string GetCellValueFromSheet(string filePath, WorkbookType sheetName, string cellReference)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var workbook = WorkbookFactory.Create(file);

                var sheet = workbook.GetSheet(sheetName.ToString());
                var cellRef = new CellReference(cellReference);

                return GetCellValueAsString(sheet.GetRow(cellRef.Row).GetCell(cellRef.Col));
            }
        }

        private List<string> ProccessSingleSheet(List<string> targetCells, string filePath, WorkbookType sheetName)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var workbook = WorkbookFactory.Create(file);

                var sheet = workbook.GetSheet(sheetName.ToString());

                return targetCells
                    .Select(c => new CellReference(c))
                    .Select(c => GetCellValueAsString(sheet.GetRow(c.Row).GetCell(c.Col)))
                    .ToList();


                //var noteStart = new CellReference(noteStartCell);
                //var valueStart = new CellReference(valueStartCell);

                //var noteColumnIndex = noteStart.Col;
                //var valueColumnIndex = valueStart.Col;

                //var startRow = noteStart.Row;
                //var valuesList = new List<string>();

                //HSSFFormulaEvaluator formula = new HSSFFormulaEvaluator(workbook);

                //for (int i = 0; i < sheet.LastRowNum - startRow; i++)
                //{
                //    var currentRow = sheet.GetRow(startRow + i);

                //    var currentCellValue = currentRow.GetCell(noteColumnIndex)?.StringCellValue;
                //    if (string.IsNullOrWhiteSpace(currentCellValue)) continue;

                //    var cell = currentRow.GetCell(valueColumnIndex);
                //    formula.EvaluateInCell(cell);

                //    var value = cell.NumericCellValue.ToString();
                //    valuesList.Add(value);
                //}

                //return valuesList;
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

                var startingColumnIndex = 0;
                Enumerable.Range(0, notesToAdd.Count())
                    .ToList()
                    .ForEach(i =>
                    {
                        var currentRow = sheet.CreateRow(startingRow + i);
                        currentRow.CreateCell(startingColumnIndex).SetCellValue(companiesArray.ElementAt(i).Key);
                        SetCompanyRow(currentRow, companiesArray.ElementAt(i).Value.ToArray());
                    });

                FileStream outputStream = new FileStream(notesFilePath, FileMode.Create);
                workbook.Write(outputStream);
                outputStream.Close();
            }
        }

        public void UpdateNotesForCompanies(IDictionary<string, List<string>> notesToOverride)
        {
            if (!notesToOverride.Any()) return;
            using (FileStream file = new FileStream(notesFilePath, FileMode.Open, FileAccess.Read))
            {
                var workbook = WorkbookFactory.Create(file);

                var sheet = workbook.GetSheetAt(0);

                var companiesProcessed = 0;
                for (int i = 3; i <= sheet.LastRowNum; i++)
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
            var skipCell = new CellReference("M4");
            var cellsToSet = new Queue<int>(Enumerable.Range(3, values.Length + 1).Where(i => i != skipCell.Col));

            for (int i = 0; i < values.Length; i++)
            {
                var columnIndex = cellsToSet.Dequeue();

                var cell = currentRow.GetCell(columnIndex);
                var value = values[i];

                if (cell == null)
                {
                    currentRow.CreateCell(columnIndex).SetCellValue(value);
                    cell = currentRow.GetCell(columnIndex);
                }
                else
                {
                    cell.SetCellValue(value);
                }

                //set currency style
                if (columnIndex >= 13)
                {
                    try
                    {
                        var stringCellValue = cell.StringCellValue;
                        var doubleVal = string.IsNullOrEmpty(stringCellValue) ? 0 : Convert.ToDouble(stringCellValue);
                        cell.SetCellValue(doubleVal);

                        var workbook = cell.Sheet.Workbook;
                        ICellStyle cs = workbook.CreateCellStyle();
                        cs.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0.00 kn");

                        cell.CellStyle = cs;
                    }
                    catch (FormatException ex)
                    {
                        var msg = $"Expected a number but found \"{cell.StringCellValue}\".\r\n" +
                            $"Invalid cell is at the address: {cell.Address}, in the bilanca.xls";
                        throw new FormatException(msg, ex);
                    }
                }
            }

            //set opis formula
            var descCell = currentRow.GetCell(skipCell.Col) ?? currentRow.CreateCell(skipCell.Col);

            descCell.SetCellType(CellType.Formula);
            descCell.SetCellFormula(string.Format("VLOOKUP(L{0}, Djel!$A$2:$B$616, 2, FALSE)", currentRow.RowNum + 1));

            new HSSFFormulaEvaluator(currentRow.Sheet.Workbook).EvaluateInCell(descCell);
        }

        private string GetCellValueAsString(ICell cell)
        {
            DataFormatter dataFormatter = new DataFormatter();
            return dataFormatter.FormatCellValue(cell, new HSSFFormulaEvaluator(cell.Sheet.Workbook));
        }
    }
}