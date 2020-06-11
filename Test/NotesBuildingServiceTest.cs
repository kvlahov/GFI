using System;
using System.Configuration;
using System.Linq;
using GFIManager.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class NotesBuildingServiceTest
    {
        [TestMethod]
        public void CompanyHasInvalidGfi()
        {
            var root = "C:/Users/evlakre/Downloads/GFI/2019";
            var dirService = new DirectoryService(root);

            var companies = dirService.GetCompaniesWithCreatedGfi();

            var sut = new NotesBuildingService(root);

            var res = sut.CompanyHasInvalidGfi(companies.First());

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void GetCompaniesWithCreatedNotes()
        {
            var root = "C:/Users/evlakre/Downloads/GFI/2019";
            var dirService = new DirectoryService(root);

            var companies = dirService.GetCompaniesWithCreatedGfi();
            var sut = new NotesBuildingService(root);
            
            var res = sut.GetCompaniesWithCreatedNotes(companies);
            
            //Assert.AreNotEqual(res.Count(), 0);

        }
    }
}
