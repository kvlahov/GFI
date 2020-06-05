using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GFIManager.Models;
using IronXL;

namespace GFIManager.Services
{
    public class GfiCreatorService
    {
        private readonly IEnumerable<Company> companies;
        private readonly IDictionary<WorkbookType, string> workbookFileNames;

        public GfiCreatorService(IEnumerable<Company> companies)
        {
            this.companies = companies;
            workbookFileNames = GetWorkbookFileNames();
        }

        private IDictionary<WorkbookType, string> GetWorkbookFileNames()
        {
            return new Dictionary<WorkbookType, string>
            {
                {WorkbookType.Bilanca, "BIL.xls" },
                {WorkbookType.RDG, "RDG.xls" },
                {WorkbookType.Dodatni, "DOD.xls" }
            };
        }

        public void BuildGfis()
        {
            ProcessSingleCompany(companies.First());
        }

        private void ProcessSingleCompany(Company company)
        {
            var filePaths = Directory.GetFiles(company.DirectoryPath);
            var startFile = filePaths.First(p => p.EndsWith("objava 2019.xls"));

            WorkBook workbook = WorkBook.Load(startFile);
            WorkSheet sheet = workbook.WorkSheets.First(s => s.Name == WorkbookType.Bilanca.ToString());

            workbook.SaveAs(Path.Combine(company.DirectoryPath, "copy.xls"));

            var val = sheet["G9:G133"].Rows;
            var testCell = sheet["J9:J133"].Rows.Where(r => !r.IsEmpty);

            testCell.ToList().ForEach(r => r.Value = 10);

            workbook.Save();
        }
    }
}
