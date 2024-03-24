using GFIManager.Models;
using GFIManager.Properties;
using Microsoft.Office.Interop.Excel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GFIManager.Services.Notes
{
    public class NotesBuildingService : OfficeBaseService
    {
        private readonly IEnumerable<Company> companies;
        private readonly IDictionary<WorkbookType, WorksheetInfo> workbooksInfo;
        private readonly IDictionary<WorkbookType, string> sourceWorksheetsRanges;

        public NotesBuildingService(IEnumerable<Company> companies)
        {
            this.companies = companies;
            workbooksInfo = GetWorkbookInfo();
            sourceWorksheetsRanges = GetSourceWorksheetRanges();
        }

        private IDictionary<WorkbookType, string> GetSourceWorksheetRanges()
        {
            return new Dictionary<WorkbookType, string>
            {
                { WorkbookType.Bilanca, "F7:I137" },
                { WorkbookType.RDG, "F7:I74" },
                { WorkbookType.Dodatni, "F8:I104" },
            };
        }

        private IDictionary<WorkbookType, WorksheetInfo> GetWorkbookInfo()
        {
            return new Dictionary<WorkbookType, WorksheetInfo>
            {
                {
                    WorkbookType.Bilanca,
                    new WorksheetInfo
                    {
                        FileName = Settings.Default.BilancaFileName,
                        Range = "G9:J135"
                    }
                },
                {
                    WorkbookType.RDG,
                    new WorksheetInfo
                    {
                        FileName = Settings.Default.RDGFileName,
                        Range = "G8:J113"
                    }
                },
                {
                    WorkbookType.Dodatni,
                    new WorksheetInfo
                    {
                        FileName = Settings.Default.DodatniFileName,
                        Range = "H9:J88"
                    }
                }
            };
        }

        public void BuildGfis()
        {
            Parallel.ForEach(companies, ProcessSingleCompany);
        }

        private void ProcessSingleCompany(Company company)
        {
            var filePaths = Directory.GetFiles(company.DirectoryPath);
            var oldGfiSuffixRegex = new Regex(Settings.Default.OldGfiSuffix);
            var startFile = filePaths.First(p => oldGfiSuffixRegex.IsMatch(p));

            var newFileName = Path.GetFileNameWithoutExtension(startFile) + Settings.Default.FinalGfiSuffix;
            var newFilePath = Path.Combine(company.DirectoryPath, newFileName);

            IWorkbook workbook;
            using (FileStream file = new FileStream(startFile, FileMode.Open, FileAccess.Read))
            {
                workbook = WorkbookFactory.Create(file);
            }

            //process each sheet
            var sheet = workbook.GetSheet(WorkbookType.Bilanca.ToString());
            ProcessSingleSheet(company.DirectoryPath, sheet, WorkbookType.Bilanca);

            sheet = workbook.GetSheet(WorkbookType.RDG.ToString());
            ProcessSingleSheet(company.DirectoryPath, sheet, WorkbookType.RDG);

            sheet = workbook.GetSheet(WorkbookType.Dodatni.ToString());
            ProcessSingleSheet(company.DirectoryPath, sheet, WorkbookType.Dodatni);

            //save
            using (FileStream outputStream = new FileStream(newFilePath, FileMode.Create))
            {
                workbook.Write(outputStream);
                outputStream.Close();
            }

            RefreshData(newFilePath);
        }

        private void RefreshData(string newFilePath)
        {
            Application app = new Application();
            var wb = app.Workbooks.Open(newFilePath);

            app.ScreenUpdating = false;
            app.DisplayAlerts = false;
            app.EnableEvents = false;
            app.Interactive = false;

            app.CalculateFull();
            wb.Close(true);

            app.Quit();
            ReleaseObject(app);
        }

        private void ProcessSingleSheet(string directoryPath, ISheet targetSheet, WorkbookType workbookType)
        {
            var filePath = Path.Combine(directoryPath, workbooksInfo[workbookType].FileName);
            targetSheet.ForceFormulaRecalculation = false;

            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var wb = WorkbookFactory.Create(file);
                var sourceSheet = wb.GetSheetAt(0);

                //get data from source sheet
                var sourceRange = CellRangeAddress.ValueOf(sourceWorksheetsRanges[workbookType]);
                var sourceValues = Enumerable.Range(sourceRange.FirstRow, sourceRange.LastRow - sourceRange.FirstRow + 1)
                    .Select(i => new
                    {
                        Aop = sourceSheet.GetRow(i).GetCell(sourceRange.FirstColumn)?.StringCellValue,
                        Value = GetCellValueAsString(sourceSheet.GetRow(i).GetCell(sourceRange.LastColumn))
                    })
                    .Where(m => !string.IsNullOrEmpty(m.Aop))
                    .Where(m => int.TryParse(m.Aop, out int _))
                    .ToDictionary(c => c.Aop, c => c.Value);

                //set data on target sheet (of final GFI)
                var targetRange = CellRangeAddress.ValueOf(workbooksInfo[workbookType].Range);
                for (int rowIndex = targetRange.FirstRow; rowIndex <= targetRange.LastRow; rowIndex++)
                {
                    if (targetSheet.GetRow(rowIndex).GetCell(targetRange.LastColumn).CellStyle.IsLocked) continue;

                    var aopDouble = targetSheet.GetRow(rowIndex).GetCell(targetRange.FirstColumn).NumericCellValue;
                    var aop = Convert.ToInt32(aopDouble).ToString("D3");

                    var aopValue = sourceValues.TryGetValue(aop, out string value) ? value : null;
                    var newValue = string.IsNullOrEmpty(aopValue) ? 0 : Convert.ToInt32(aopValue);

                    targetSheet.GetRow(rowIndex).GetCell(targetRange.LastColumn).SetCellValue(newValue);
                }
            }

            targetSheet.ForceFormulaRecalculation = true;
        }

        private string GetCellValueAsString(ICell cell)
        {
            DataFormatter dataFormatter = new DataFormatter();
            return dataFormatter.FormatCellValue(cell);
        }
    }
}