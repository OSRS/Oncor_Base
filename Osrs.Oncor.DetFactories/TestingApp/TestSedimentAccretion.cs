using System;
using ExcelDETs.DETs;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace TestingApp
{
    static class TestSedimentAccretion
    {
        public static void ReadFile(string fName)
        {
            Console.WriteLine("reading workbook named [{0}]", fName);
            SedimentAccretionDET det = new SedimentAccretionDET();
            ExcelSedimentAccretionDET excel = new ExcelSedimentAccretionDET(det);
            excel.Load(fName);
            ValidationIssues issues = det.ValidationIssues;
            if (issues.Count > 0)
            {
                foreach (ValidationIssue issue in issues)
                {
                    Console.WriteLine(issue.IssueMessage);
                }
                return;
            }
            Console.WriteLine("Custom property Name: {0}, Value: {1}", "oncorID", det.Id);
            Console.WriteLine("Custom property Name: {0}, Value: {1}", "oncorOwner", det.Owner);
            int count = 1;
            foreach (var dto in det.Surveys.Values)
            {
                Console.WriteLine("Survey Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Elevations.Values)
            {
                Console.WriteLine("Elevation Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Sites.Values)
            {
                Console.WriteLine("Site Row: {0}, Value: {1}", count++, dto);
            }
            Console.WriteLine("Closing workbook named [{0}]", fName);
        }
        public static void WriteFile(string fName)
        {
            SedimentAccretionDET det = new SedimentAccretionDET();
            det.Id = Guid.NewGuid();
            det.Owner = "Dr. Frank N. Furter, ESQ";
            ExcelSedimentAccretionDET excel = new ExcelSedimentAccretionDET(det);
            CreateListOfPhonySurveys(det, 4);
            CreateListOfPhonyElevations(det, 4);
            CreateListOfPhonySites(det, 4);
            excel.Save(fName);
        }

        private static void CreateListOfPhonySurveys(SedimentAccretionDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonySurvey(count++);
                det.Surveys.Add(dto);
            }
        }

        private static void CreateListOfPhonyElevations(SedimentAccretionDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyElevation(count++);
                det.Elevations.Add(dto);
            }
        }

        private static void CreateListOfPhonySites(SedimentAccretionDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonySite(count++);
                det.Sites.Add(dto);
            }
        }

        private static SedimentAccretionSurvey CreatePhonySurvey(int index)
        {
            SedimentAccretionSurvey dto = new SedimentAccretionSurvey();
            dto.SurveyId = string.Format("SurveyId {0}", index);
            dto.SiteId = string.Format("SiteId {0}", index);
            dto.DateTime = Parsing.ParseDate(string.Format("{0}/01/2017", index));
            dto.ElevTopA = Parsing.ParseDouble("");
            dto.ElevTopB = Parsing.ParseDouble("");
            dto.Comments = string.Format("Comments {0}", index);
            return dto;
        }

        private static SedimentAccretionElevation CreatePhonyElevation(int index)
        {
            SedimentAccretionElevation dto = new SedimentAccretionElevation();
            dto.SurveyId = string.Format("SurveyId {0}", index);
            dto.VertCmDown = Parsing.ParseDouble("");
            dto.HorizCmFromA = Parsing.ParseDouble("");
            dto.Comments = string.Format("Comments {0}", index);
            return dto;
        }
    }
}
