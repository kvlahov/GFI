using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Test
{
    [TestClass]
    public class NotesBuildingServiceTest
    {
        private NotesBuildingService sut;
        private string root;

        [TestInitialize]
        public void TestInitialize()
        {
            root = "C:/Users/evlakre/Downloads/GFI/2019";
            sut = new NotesBuildingService(root);
        }

        [TestMethod]
        public void CompanyHasInvalidGfi()
        {
            var dirService = new DirectoryService(root);
            var companies = dirService.GetCompaniesWithCreatedGfi();

            var res = sut.CompanyHasInvalidGfi(companies.First());

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void GetCompaniesWithCreatedNotes()
        {
            var dirService = new DirectoryService(root);

            var companies = dirService.GetCompaniesWithCreatedGfi();

            var res = sut.GetCompaniesWithCreatedNotes(companies);

            //Assert.AreNotEqual(res.Count(), 0);
        }

        [TestMethod]
        public void AddNotesForCompanies()
        {
            var dirService = new DirectoryService(root);
            var companies = dirService.GetCompaniesWithCreatedGfi();

            var companyData = sut.GetDataForNotes(companies);

            sut.AddNotesForCompanies(companyData);
        }

        [TestMethod]
        public void UpdateNotesForCompanies()
        {
            var dirService = new DirectoryService(root);
            var companies = dirService.GetCompaniesWithCreatedGfi();

            var companyData = sut.GetDataForNotes(companies.Where(c => c.DisplayName.ToLower() == "adria libar"));
            sut.UpdateNotesForCompanies(companyData);
        }

        [TestMethod]
        public void ProcessSingleCompnay()
        {
            var dirService = new DirectoryService(root);
            var companies = dirService.GetCompaniesWithCreatedGfi();

            var res = sut.ProcessSingleCompany(companies.First(c => c.DisplayName.ToLower() == "adria libar"));
        }
    }
}