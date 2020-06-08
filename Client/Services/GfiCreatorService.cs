using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop;
using GFIManager.Models;
using IronXL;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DocumentFormat.OpenXml.Bibliography;
using IronXL.Xml.Wordprocessing;

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
                        FileName = "bil.xls",
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
                        FileName = "rdg.xls",
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
                        FileName = "dod.xls",
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
            companies.ToList().ForEach(ProcessSingleCompany);
        }

        private void ProcessSingleCompany(Company company)
        {
            var filePaths = Directory.GetFiles(company.DirectoryPath);
            var startFile = filePaths.First(p => p.EndsWith("objava 2019.xls"));

            var newFileName = Path.GetFileNameWithoutExtension(startFile) + "-final" + ".xls";


            //todo catch exception if file is used by another process
            WorkBook workbook = WorkBook.Load(startFile);
            var newFilePath = Path.Combine(company.DirectoryPath, newFileName);            

            workbook.SaveAs(newFilePath);
            workbook.Close();
            
            workbook = WorkBook.Load(newFilePath);

            WorkSheet sheet = workbook.WorkSheets.First(s => s.Name == WorkbookType.Bilanca.ToString());
            ProcessSingleSheet(company.DirectoryPath, sheet, WorkbookType.Bilanca);

            sheet = workbook.WorkSheets.First(s => s.Name == WorkbookType.RDG.ToString());
            ProcessSingleSheet(company.DirectoryPath, sheet, WorkbookType.RDG);

            sheet = workbook.WorkSheets.First(s => s.Name == WorkbookType.Dodatni.ToString());
            ProcessSingleSheet(company.DirectoryPath, sheet, WorkbookType.Dodatni);

            workbook.Save();
            workbook.Close();

            RefreshCalculatedCells(newFilePath);
        }
        
        private void ProcessSingleSheet(string directoryPath, WorkSheet targetSheet, WorkbookType workbookType)
        {
            var filePath = Path.Combine(directoryPath, workbooksInfo[workbookType].FileName);
            var workbook = WorkBook.Load(filePath);

            var sourceSheet = workbook.WorkSheets.First();
            var range = workbooksInfo[workbookType].Range;

            targetSheet[range].Rows
                .Where(r => !workbooksInfo[workbookType].LockedAops.Contains(r.Columns.First().Value.ToString()))
                .Where(r => !r.Columns.First().IsEmpty)
                .Select(r => new { Aop = r.Columns.First(), CurrentYear = r.Columns.Last() })
                .ToList()
                .ForEach(r =>
                {
                    var sourceRange = sourceWorksheetsRanges[workbookType];
                    var value = sourceSheet[sourceRange].Rows.First(row => row.Columns.First().StringValue.TrimEnd() == r.Aop.IntValue.ToString("D3")).Columns.Last().DoubleValue;
                    r.CurrentYear.DoubleValue = value;
                });

            workbook.Close();
        }

        private void RefreshCalculatedCells(string newFilePath)
        {
            Application xlApp = new Application();
            Workbook xlWorkbook = xlApp.Workbooks.Open(newFilePath);

            Microsoft.Office.Interop.Excel.Range cells = xlWorkbook.Sheets["Bilanca"].Range["J9:J133"].Cells;
            RefreshSheet(cells);

            cells = xlWorkbook.Sheets["RDG"].Range["J9:J105"].Cells;
            RefreshSheet(cells);

            cells = xlWorkbook.Sheets["Dodatni"].Range["J9:J88"].Cells;
            RefreshSheet(cells);

            xlApp.DisplayAlerts = false;
            xlWorkbook.Close(true);
            xlApp.Quit();

            ReleaseObject(cells);
            ReleaseObject(xlWorkbook);
            ReleaseObject(xlApp);
        }

        private void RefreshSheet(Microsoft.Office.Interop.Excel.Range cells)
        {
            foreach (Microsoft.Office.Interop.Excel.Range cell in cells)
            {
                if (!cell.Locked)
                {
                    var value = cell.Value2;
                    cell.Value = value;
                }
            }
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

        private void ReadLockedCells(string filePath)
        {
            Application xlApp = new Application();
            Workbook xlWorkbook = xlApp.Workbooks.Open(filePath);
            _Worksheet xlWorksheet = xlWorkbook.Sheets["Bilanca"];
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheet.Range["J9", "J133"];

            var lockedCellsBil = new List<string>();
            foreach (Microsoft.Office.Interop.Excel.Range cell in xlRange.Cells)
            {
                if (cell.Locked)
                {
                    var val = Convert.ToString(xlWorksheet.Cells[cell.Row, 7].Value);
                    lockedCellsBil.Add(val);
                }
            }

            xlWorksheet = xlWorkbook.Sheets["RDG"];
            xlRange = xlWorksheet.Range["J8", "J105"];

            var lockedCellsRDG = new List<string>();
            foreach (Microsoft.Office.Interop.Excel.Range cell in xlRange.Cells)
            {
                if (cell.Locked)
                {
                    var val = Convert.ToString(xlWorksheet.Cells[cell.Row, 7].Value);
                    lockedCellsRDG.Add(val);
                }
            }

            xlWorksheet = xlWorkbook.Sheets["Dodatni"];
            xlRange = xlWorksheet.Range["J9", "J88"];

            var lockedCellsDod = new List<string>();
            foreach (Microsoft.Office.Interop.Excel.Range cell in xlRange.Cells)
            {
                if (cell.Locked)
                {
                    var val = Convert.ToString(xlWorksheet.Cells[cell.Row, 8].Value);
                    lockedCellsDod.Add(val);
                }
            }

            var bilancaCells = string.Join(", ", lockedCellsBil.Where(s => !string.IsNullOrEmpty(s)).Select(s => $"\"{s}\""));
            var rdgCells = string.Join(", ", lockedCellsRDG.Where(s => !string.IsNullOrEmpty(s)).Select(s => $"\"{s}\""));
            var dodatniCells = string.Join(", ", lockedCellsDod.Where(s => !string.IsNullOrEmpty(s)).Select(s => $"\"{s}\""));

            Debug.WriteLine(bilancaCells);
            Debug.WriteLine(rdgCells);
            Debug.WriteLine(dodatniCells);

            xlWorkbook.Close(false);
            xlApp.Quit();

            ReleaseObject(xlRange);
            ReleaseObject(xlWorksheet);
            ReleaseObject(xlWorkbook);
            ReleaseObject(xlApp);
        }

    }
}
