using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop;
using GFIManager.Models;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GFIManager.Properties;

namespace GFIManager.Services
{
    public class GfiCreatorService
    {
        private readonly IEnumerable<Company> companies;
        private readonly IDictionary<WorkbookType, WorksheetInfo> workbooksInfo;
        private readonly IDictionary<WorkbookType, string> sourceWorksheetsRanges;

        public GfiCreatorService(IEnumerable<Company> companies)
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

            var newFileName = Path.GetFileNameWithoutExtension(startFile) + Settings.Default.FinalGfiSuffix + ".xls";

            Application xlApp = new Application();
            Workbook xlWorkbook = xlApp.Workbooks.Open(startFile);
            xlApp.DisplayAlerts = false;
            xlApp.ScreenUpdating = false;
            xlApp.Calculation = XlCalculation.xlCalculationManual;

            var newFilePath = Path.Combine(company.DirectoryPath, newFileName);

            //create copy and load it
            xlWorkbook.SaveCopyAs(newFilePath);
            xlWorkbook.Close(false);
            xlWorkbook = xlApp.Workbooks.Open(newFilePath);

            //process each sheet
            _Worksheet xlWorksheet = xlWorkbook.Sheets[WorkbookType.Bilanca.ToString()];
            ProcessSingleSheet(company.DirectoryPath, xlWorksheet, xlApp, WorkbookType.Bilanca);

            xlWorksheet = xlWorkbook.Sheets[WorkbookType.RDG.ToString()];
            ProcessSingleSheet(company.DirectoryPath, xlWorksheet, xlApp, WorkbookType.RDG);

            xlWorksheet = xlWorkbook.Sheets[WorkbookType.Dodatni.ToString()];
            ProcessSingleSheet(company.DirectoryPath, xlWorksheet, xlApp, WorkbookType.Dodatni);

            xlApp.Calculation = XlCalculation.xlCalculationAutomatic;
            xlApp.Calculate();

            xlWorkbook.Close(true);
            xlApp.Quit();

            ReleaseObject(xlWorkbook);
            ReleaseObject(xlApp);
        }

        private void ProcessSingleSheet(string directoryPath, _Worksheet targetSheet, Application xlApp, WorkbookType workbookType)
        {
            var filePath = Path.Combine(directoryPath, workbooksInfo[workbookType].FileName);
            var workbook = xlApp.Workbooks.Open(filePath);

            _Worksheet sourceSheet = workbook.Sheets[1];
            var range = workbooksInfo[workbookType].Range;

            var columnsCount = targetSheet.Range[range].Rows[1].Columns.Count;
            targetSheet.Range[range].Rows.Cast<Range>()
                .Where(r => !r.Cells[columnsCount].Locked)
                .Select(r => new
                {
                    Aop = Convert.ToInt32(r.Cells[1].Value).ToString("D3"),
                    CurrentYear = r.Cells[columnsCount]
                })
                .ToList()
                .ForEach(r =>
                {
                    var sourceRange = sourceWorksheetsRanges[workbookType];
                    var value = sourceSheet.Range[sourceRange].Rows.Cast<Range>().First(row => Convert.ToString(row.Columns[1].Value) == r.Aop).Columns.Cast<Range>().Last().Value;
                    r.CurrentYear.Value = Convert.ToInt32(value);
                });

            workbook.Close();
            ReleaseObject(sourceSheet);
            ReleaseObject(workbook);
        }
        private void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
