using ExcelDETs.DETs;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using System;

namespace TestingApp
{
    static class TestVeg
    {
        public static void ReadFile(string fName)
        {
            Console.WriteLine("reading workbook named [{0}]", fName);
            VegDET det = new VegDET();
            ExcelVegDET excel = new ExcelVegDET(det);
            excel.Load(fName);
            ValidationIssues issues = det.ValidationIssues;
            if (issues.Count > 0)
            {
                foreach (ValidationIssue issue in issues)
                {
                    Console.WriteLine(issue.IssueMessage);
                }
                //return;
            }
            Console.WriteLine("Custom property Name: {0}, Value: {1}", "oncorID", det.Id);
            Console.WriteLine("Custom property Name: {0}, Value: {1}", "oncorOwner", det.Owner);
            int count = 1;
            foreach (var dto in det.Sites.Values)
            {
                Console.WriteLine("Site Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Elevations.Values)
            {
                Console.WriteLine("Elevations Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Herbs.Values)
            {
                Console.WriteLine("Herbs Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.HerbSpecies.Values)
            {
                Console.WriteLine("HerbSpecies Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.NonLiving.Values)
            {
                Console.WriteLine("NonLiving Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.PlotTypes.Values)
            {
                Console.WriteLine("PlotTypes Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Shrubs.Values)
            {
                Console.WriteLine("Shrubs Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.ShrubSpecies.Values)
            {
                Console.WriteLine("Shrubs Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Surveys.Values)
            {
                Console.WriteLine("Surveys Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Trees.Values)
            {
                Console.WriteLine("Trees Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.TreeSpecies.Values)
            {
                Console.WriteLine("TreeSpecies Row: {0}, Value: {1}", count++, dto);
            }
            Console.WriteLine("Closing workbook named [{0}]", fName);
        }

        public static void WriteFile(string fName)
        {
            VegDET det = new VegDET();
            det.Id = Guid.NewGuid();
            det.Owner = "Dr. Frank N. Furter, ESQ";
            ExcelVegDET excel = new ExcelVegDET(det);
            CreateListOfPhonySites(det, 4);
            excel.Save(fName);
        }

        private static void CreateListOfPhonySites(VegDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonySite(count++);
                det.Sites.Add(dto);
            }
        }
    }
}
