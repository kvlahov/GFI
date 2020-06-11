using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GFIManager.Models;
using GFIManager.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class GfiCreatorServiceTest
    {
        private GfiBuilderService service;
        private IEnumerable<Company> allCompanies;

        [TestInitialize]
        public void TestInitialize()
        {
            var root = "C:/Users/evlakre/Downloads/GFI/2019";
            allCompanies = Directory.GetDirectories(root).Select(d => new Company(d));
        }


        [TestMethod]
        public void BuildGfiTest()
        {
            service = new GfiBuilderService(new List<Company> { allCompanies.First() });

            service.BuildGfis();
        }
    }
}
