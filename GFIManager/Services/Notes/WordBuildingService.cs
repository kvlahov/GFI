﻿using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GFIManager.Services.Notes
{
    public class WordBuildingService : OfficeBaseService
    {
        public WordBuildingService()
        {
            object missing = Type.Missing;
            app = new Application();
            doc = app.Documents.Open("C:\\Users\\Kreso\\Downloads\\GFI 2024\\biljeske_template.docx", ref missing, true);
        }

        private Application app;
        private Document doc;
        private Bookmarks bookmarks;

        public void TestFilling()
        {
            //bookmarks = doc.Bookmarks;

            try
            {
                GfiFindAndReplaceList.ForEach(replacableText =>
                {
                    var cell = replacableText.Split(' ').Last();
                    FindAndReplace(replacableText, $"test {cell}");
                });

                doc.SaveAs2("C:\\Users\\Kreso\\Downloads\\GFI 2024\\Test_biljeske.docx");
            }
            finally
            {
                app.Quit();
                ReleaseObject(app);
            }
        }

        private void FindAndReplace(string textToFind, string replacementText)
        {
            Find find = app.Selection.Find;
            find.Text = textToFind;
            find.Replacement.Text = replacementText;
            var range = doc.Content;
            find.Execute(Replace: WdReplace.wdReplaceAll);
            //while (find.Execute(Replace: WdReplace.wdReplaceAll))
            //{
            //    if (range.HighlightColorIndex != WdColorIndex.wdNoHighlight)
            //    {
            //        range.HighlightColorIndex = WdColorIndex.wdNoHighlight;
            //    }
            //}
        }

        private void SetTextOnBookmark(string bookmarkKey, string text)
        {
            var range = bookmarks[bookmarkKey].Range;
            range.Text = text;
            range.HighlightColorIndex = WdColorIndex.wdNoHighlight;
        }

        private List<string> GfiFindAndReplaceList = new List<string>()
        {
            "GFI RefStr C29",
            "GFI RefStr C33",
            "GFI RefStr C31",
            "GFI RefStr F4",
            "GFI RefStr C29",
            "GFI RefStr D7",
            "GFI RefStr M27",
            "GFI RefStr C27",
            "GFI RefStr H27",
            "GFI RefStr F31",
            "GFI RefStr C42",
            "GFI RefStr D42",
            "GFI RefStr A75",
            "GFI RefStr F56",
            "GFI RefStr C56",
            "GFI RefStr F12",
            "GFI RefStr D50",
            "GFI RDG I9",
            "GFI RDG J9",
            "GFI RDG I10",
            "GFI RDG J10",
            "GFI RDG I11",
            "GFI RDG J11",
            "GFI RDG I12",
            "GFI RDG J12",
            "GFI RDG I13",
            "GFI RDG J13",
            "GFI RDG I8",
            "GFI RDG J8",
            "GFI RDG I15",
            "GFI RDG J15",
            "GFI RDG I16",
            "GFI RDG J16",
            "GFI RDG I17",
            "GFI RDG J17",
            "GFI RDG I18",
            "GFI RDG J18",
            "GFI RDG I19",
            "GFI RDG J19",
            "GFI RDG I20",
            "GFI RDG J20",
            "GFI RDG I21",
            "GFI RDG J21",
            "GFI RDG I22",
            "GFI RDG J22",
            "GFI RDG I23",
            "GFI RDG J23",
            "GFI RDG I24",
            "GFI RDG J24",
            "GFI RDG I25",
            "GFI RDG J25",
            "GFI RDG I26",
            "GFI RDG J26",
            "GFI RDG I27",
            "GFI RDG J27",
            "GFI RDG I28",
            "GFI RDG J28",
            "GFI RDG I29",
            "GFI RDG J29",
            "GFI RDG I30",
            "GFI RDG J30",
            "GFI RDG I31",
            "GFI RDG J31",
            "GFI RDG I32",
            "GFI RDG J32",
            "GFI RDG I33",
            "GFI RDG J33",
            "GFI RDG I34",
            "GFI RDG J34",
            "GFI RDG I35",
            "GFI RDG J35",
            "GFI RDG I36",
            "GFI RDG J36",
            "GFI RDG I14",
            "GFI RDG J14",
            "GFI RDG I38",
            "GFI RDG J38",
            "GFI RDG I39",
            "GFI RDG J39",
            "GFI RDG I40",
            "GFI RDG J40",
            "GFI RDG I41",
            "GFI RDG J41",
            "GFI RDG I42",
            "GFI RDG J42",
            "GFI RDG I43",
            "GFI RDG J43",
            "GFI RDG I44",
            "GFI RDG J44",
            "GFI RDG I45",
            "GFI RDG J45",
            "GFI RDG I46",
            "GFI RDG J46",
            "GFI RDG I47",
            "GFI RDG J47",
            "GFI RDG I37",
            "GFI RDG J37",
            "GFI RDG I49",
            "GFI RDG J49",
            "GFI RDG I50",
            "GFI RDG J50",
            "GFI RDG I51",
            "GFI RDG J51",
            "GFI RDG I52",
            "GFI RDG J52",
            "GFI RDG I53",
            "GFI RDG J53",
            "GFI RDG I54",
            "GFI RDG J54",
            "GFI RDG I55",
            "GFI RDG J55",
            "GFI RDG I48",
            "GFI RDG J48",
            "GFI RDG J60",
            "GFI RDG I60",
            "GFI RDG J61",
            "GFI RDG I61",
            "GFI RDG I66",
            "GFI RDG J65",
            "GFI RDG J66",
            "GFI Bilanca I29",
            "GFI Bilanca J29",
            "GFI Bilanca I30",
            "GFI Bilanca J30",
            "GFI Bilanca I31",
            "GFI Bilanca J31",
            "GFI Bilanca I32",
            "GFI Bilanca J32",
            "GFI Bilanca I33",
            "GFI Bilanca J33",
            "GFI Bilanca I34",
            "GFI Bilanca J34",
            "GFI Bilanca I35",
            "GFI Bilanca J35",
            "GFI Bilanca I36",
            "GFI Bilanca J36",
            "GFI Bilanca I37",
            "GFI Bilanca J37",
            "GFI Bilanca I38",
            "GFI Bilanca J38",
            "GFI Bilanca I28",
            "GFI Bilanca J39",
            "GFI Bilanca I40",
            "GFI Bilanca J40",
            "GFI Bilanca I41",
            "GFI Bilanca J41",
            "GFI Bilanca I42",
            "GFI Bilanca J42",
            "GFI Bilanca I43",
            "GFI Bilanca J43",
            "GFI Bilanca I39",
            "GFI Bilanca J39",
            "GFI Bilanca I47",
            "GFI Bilanca J47",
            "GFI Bilanca I48",
            "GFI Bilanca J48",
            "GFI Bilanca I49",
            "GFI Bilanca J49",
            "GFI Bilanca I50",
            "GFI Bilanca J50",
            "GFI Bilanca I51",
            "GFI Bilanca J51",
            "GFI Bilanca I52",
            "GFI Bilanca J52",
            "GFI Bilanca I53",
            "GFI Bilanca J53",
            "GFI Bilanca I46",
            "GFI Bilanca J46",
            "GFI Bilanca I62",
            "GFI Bilanca J62",
            "GFI Bilanca I63",
            "GFI Bilanca J63",
            "GFI Bilanca I64",
            "GFI Bilanca J64",
            "GFI Bilanca I65",
            "GFI Bilanca J65",
            "GFI Bilanca I66",
            "GFI Bilanca J66",
            "GFI Bilanca I67",
            "GFI Bilanca J67",
            "GFI Bilanca I68",
            "GFI Bilanca J68",
            "GFI Bilanca I69",
            "GFI Bilanca J69",
            "GFI Bilanca I70",
            "GFI Bilanca J70",
            "GFI Bilanca I61",
            "GFI Bilanca J61",
            "GFI Bilanca J77",
            "GFI Bilanca J78",
            "GFI Bilanca J79",
            "GFI Bilanca J85",
            "GFI Bilanca J86",
            "GFI Bilanca J92",
            "GFI Bilanca J95",
            "GFI Bilanca J99",
            "GFI Bilanca I107",
            "GFI Bilanca J107",
            "GFI Bilanca I108",
            "GFI Bilanca J108",
            "GFI Bilanca I109",
            "GFI Bilanca J109",
            "GFI Bilanca I110",
            "GFI Bilanca J110",
            "GFI Bilanca I111",
            "GFI Bilanca J111",
            "GFI Bilanca I112",
            "GFI Bilanca J112",
            "GFI Bilanca I113",
            "GFI Bilanca J113",
            "GFI Bilanca I114",
            "GFI Bilanca J114",
            "GFI Bilanca I115",
            "GFI Bilanca J115",
            "GFI Bilanca I116",
            "GFI Bilanca J116",
            "GFI Bilanca I117",
            "GFI Bilanca J117",
            "GFI Bilanca I106",
            "GFI Bilanca J106",
            "GFI Bilanca I119",
            "GFI Bilanca J119",
            "GFI Bilanca I120",
            "GFI Bilanca J120",
            "GFI Bilanca I121",
            "GFI Bilanca J121",
            "GFI Bilanca I122",
            "GFI Bilanca J122",
            "GFI Bilanca I123",
            "GFI Bilanca J123",
            "GFI Bilanca I124",
            "GFI Bilanca J124",
            "GFI Bilanca I125",
            "GFI Bilanca J125",
            "GFI Bilanca I126",
            "GFI Bilanca J126",
            "GFI Bilanca I127",
            "GFI Bilanca J127",
            "GFI Bilanca I128",
            "GFI Bilanca J128",
            "GFI Bilanca I129",
            "GFI Bilanca J129",
            "GFI Bilanca I130",
            "GFI Bilanca J130",
            "GFI Bilanca I131",
            "GFI Bilanca J131",
            "GFI Bilanca I132",
            "GFI Bilanca J132",
            "GFI Bilanca I118",
            "GFI Bilanca J118",
            "GFI RefStr F12",
            "GFI RefStr A75",
        };

        private List<string> PDFindAndReplaceList = new List<string>()
        {
            "PD I64",
            "PD I39",
            "PD I14",
            "PD I15",
            "PD I16",
            "PD I17",
            "PD I18",
            "PD I20",
            "PD I21",
            "PD I22",
            "PD I23",
            "PD I24",
            "PD I25",
            "PD I26",
            "PD I27",
            "PD I28",
            "PD I29",
            "PD I31",
            "PD I32",
            "PD I33",
            "PD I34",
            "PD I35",
            "PD I37",
            "PD I53",
            "PD I40",
            "PD I41",
            "PD I42",
            "PD I43",
            "PD I45",
            "PD I46",
            "PD I48",
            "PD I49",
            "PD I65",
            "PD I74",
            "PD I87",
            "PD I100",
            "PD I114",
            "PD I127",
            "PD I75",
            "PD I88",
            "PD I101",
            "PD I115",
            "PD I128",
            "PD I76",
            "PD I89",
            "PD I102",
            "PD I116",
            "PD I129",
            "PD I77",
            "PD I90",
            "PD I103",
            "PD I117",
            "PD I130",
            "PD I79",
            "PD I92",
            "PD I105",
            "PD I119",
            "PD I132",
            "PD I80",
            "PD I93",
            "PD I107",
            "PD I120",
            "PD I133",
            "PD I81",
            "PD I94",
            "PD I108",
            "PD I121",
            "PD I134",
            "PD I82",
            "PD I95",
            "PD I109",
            "PD I122",
            "PD I135",
            "PD I84",
            "PD I97",
            "PD I111",
            "PD I124",
            "PD I137",
            "PD I85",
            "PD I98",
            "PD I112",
            "PD I125",
            "PD I138",
        };

        private List<string> POFindAndReplaceList = new List<string>()
        {
            "OP J7",
            "OP K7",
            "OP J8",
            "OP K8",
            "OP J9",
            "OP K9",
            "OP J10",
            "OP K10",
            "OP J11",
            "OP K11",
            "OP J12",
            "OP K12",
            "OP J13",
            "OP K13",
            "OP J14",
            "OP K14",
            "OP J15",
            "OP K15",
            "OP J16",
            "OP K16",
            "OP J17",
            "OP K17",
            "OP J18",
            "OP K18",
            "OP J19",
            "OP K19",
            "OP J20",
            "OP K20",
            "OP J21",
            "OP K21",
            "OP J22",
            "OP K22",
            "OP J23",
            "OP K23",
            "OP J24",
            "OP K24",
            "OP J25",
            "OP K25",
            "OP J26",
            "OP K26",
            "OP J27",
            "OP K27",
            "OP J28",
            "OP K28",
            "OP J30",
            "OP K30",
            "OP J31",
            "OP K31",
            "OP J32",
            "OP K32",
            "OP J33",
            "OP K33",
            "OP K39",
            "OP K40",

        };
    }
}