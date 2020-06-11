using GFIManager.Models;
using GFIManager.Properties;
using Microsoft.Office.Interop.Excel;
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
        public bool CompanyHasInvalidGfi(Company company)
        {
            var filePaths = Directory.GetFiles(company.DirectoryPath);
            var startFile = filePaths.First(p => p.EndsWith(Settings.Default.FinalGfiSuffix));


            Application xlApp = new Application();
            Workbook xlWorkbook = null;
            try
            {
                xlWorkbook = xlApp.Workbooks.Open(startFile);
                xlApp.DisplayAlerts = false;
                xlApp.ScreenUpdating = false;

                var controlValue = xlWorkbook.Sheets["RefStr"].Range["A78"].Value?.ToString() as string;

                return !string.IsNullOrEmpty(controlValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                xlWorkbook?.Close();
                xlApp.Quit();

                ReleaseObject(xlWorkbook);
                ReleaseObject(xlApp);
            }

        }

        public async Task<IEnumerable<Company>> GetCompaniesWithCreatedNotes(string root, IEnumerable<Company> companies)
        {
            var notesFilePath = Directory
                .GetFiles(root)
                .First(f => f.EndsWith(Settings.Default.BiljeskeFileName));

            var companiesWithNotes = await Task.Run(() =>
            {
                Application xlApp = new Application();
                Workbook xlWorkbook = xlApp.Workbooks.Open(notesFilePath);
                xlApp.DisplayAlerts = false;
                xlApp.ScreenUpdating = false;

                Range companyNameColumn = xlWorkbook.Sheets[1].Columns[1];
                var companyNamesWithNotes = new List<string>();

                foreach (Range cell in companyNameColumn.Rows)
                {
                    if (string.IsNullOrWhiteSpace(cell.Value2?.ToString())) break;
                    companyNamesWithNotes.Add(cell.Value2.ToString());
                }


                xlWorkbook.Close();
                xlApp.Quit();

                ReleaseObject(companyNameColumn);
                ReleaseObject(xlWorkbook);
                ReleaseObject(xlApp);

                return companyNamesWithNotes;
            });

            return companies.Where(c => companiesWithNotes.Contains(c.DisplayName));
        }
    }
}
