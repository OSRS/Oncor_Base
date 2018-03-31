using System;
using ExcelDETs.DETs;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace TestingApp
{
    static class TestCrossSection
    {
        public static void ReadFile(string fName)
        {
            Console.WriteLine("reading workbook named [{0}]", fName);
            CrossSectionDET det = new CrossSectionDET();
            ExcelCrossSectionDET excel = new ExcelCrossSectionDET(det);
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
            foreach (var dto in det.Instruments.Values)
            {
                Console.WriteLine("Instruments Row: {0}, Value: {1}", count++, dto);
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
            CrossSectionDET det = new CrossSectionDET();
            det.Id = Guid.NewGuid();
            det.Owner = "Dr. Frank N. Furter, ESQ";
            ExcelCrossSectionDET excel = new ExcelCrossSectionDET(det);
            CreateListOfPhonySurveys(det, 4);
            CreateListOfPhonyElevations(det, 6);
            CreateListOfPhonySites(det, 4);
            CreateListOfPhonyInstruments(det, 6);
            excel.Save(fName);
        }

        private static void CreateListOfPhonySurveys(CrossSectionDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonySurvey(count++);
                det.Surveys.Add(dto);
            }
        }

        private static void CreateListOfPhonyElevations(CrossSectionDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyElevation(count++);
                det.Elevations.Add(dto);
            }
        }

        private static void CreateListOfPhonyInstruments(CrossSectionDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonyInstrument(count++);
                det.Instruments.Add(dto);
            }
        }

        private static void CreateListOfPhonySites(CrossSectionDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonySite(count++);
                det.Sites.Add(dto);
            }
        }

        private static CrossSectionSurveyDTO CreatePhonySurvey(int index)
        {
            CrossSectionSurveyDTO dto = new CrossSectionSurveyDTO();
            dto.SurveyId = string.Format("SurveyId {0}", index);
            dto.SiteId = string.Format("SiteId {0}", index);
            dto.InstrumentId = string.Format("InstrumentId {0}", index);
            dto.DateTime = Parsing.ParseDate(string.Format("{0}/01/2017", index));
            dto.OriginX = Parsing.ParseDouble("");
            dto.OriginY = Parsing.ParseDouble("");
            dto.DestinationX = Parsing.ParseDouble("");
            dto.DestinationY = Parsing.ParseDouble("");
            dto.Comments = string.Format("Comments {0}", index);
            return dto;
        }

        private static CrossSectionElevationDTO CreatePhonyElevation(int index)
        {
            CrossSectionElevationDTO dto = new CrossSectionElevationDTO();
            dto.SurveyId = string.Format("SurveyId {0}", index);
            dto.DistanceFromOrigin = Parsing.ParseDouble("");
            dto.Elevation = Parsing.ParseDouble("");
            dto.Comments = string.Format("Comments {0}", index);
            return dto;
        }
    }
}
