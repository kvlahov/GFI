using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GFIManager.Models;
using GFIManager.Properties;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace GFIManager.Services
{
    public class GfiBuilderService
    {
        private readonly IEnumerable<Company> companies;
        private readonly IDictionary<WorkbookType, WorksheetInfo> workbooksInfo;
        private readonly IDictionary<WorkbookType, string> sourceWorksheetsRanges;

        public GfiBuilderService(IEnumerable<Company> companies)
        {
            this.companies = companies;
            workbooksInfo = GetWorkbookInfo();
            sourceWorksheetsRanges = GetSourceWorksheetRanges();
        }

        private IDictionary<WorkbookType, string> GetSourceWorksheetRanges()
        {
            return new Dictionary<WorkbookType, string>
            {
                { WorkbookType.Bilanca, "F7:I135" },
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
                        Range = "G9:J133",
                        LockedAops = new List<string>
                        {
                            "2", "3", "10", "20", "31", "37", "38", "46", "53",
                            "65", "67", "70", "77", "81", "84", "88", "95", "107", "123"
                        }
                    }
                },
                {
                    WorkbookType.RDG,
                    new WorksheetInfo
                    {
                        FileName = Settings.Default.RDGFileName,
                        Range = "G8:J68",
                        LockedAops = new List<string>
                        {
                            "125", "131", "133", "137", "143", "146", "154", "165", "177", "178",
                            "179", "180", "181", "183", "184", "185", "186", "190", "191", "192",
                            "193", "194", "195", "196", "197", "198", "199", "203", "213", "214", "215"
                        }
                    }
                },
                {
                    WorkbookType.Dodatni,
                    new WorksheetInfo
                    {
                        FileName = Settings.Default.DodatniFileName,
                        Range = "H9:J88",
                        LockedAops = new List<string>
                        {
                            "278"
                        }
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
            var startFile = filePaths.First(p => p.EndsWith(Settings.Default.OldGfiSuffix));

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
                for (int i = targetRange.FirstRow; i <= targetRange.LastRow; i++)
                {
                    if (targetSheet.GetRow(i).GetCell(targetRange.LastColumn).CellStyle.IsLocked) continue;
                    var aopDouble = targetSheet.GetRow(i).GetCell(targetRange.FirstColumn).NumericCellValue;
                    var aop = Convert.ToInt32(aopDouble).ToString("D3");
                    var newValue = string.IsNullOrEmpty(sourceValues[aop]) ? 0 : Convert.ToInt32(sourceValues[aop]);
                    targetSheet.GetRow(i).GetCell(targetRange.LastColumn).SetCellValue(newValue);
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
