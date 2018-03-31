using System;
using ExcelDETs.DETs;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace TestingApp
{
    static class Test_PreyAvailability
    {
        public static void ReadFile(string fName)
        {
            Console.WriteLine("reading workbook named [{0}]", fName);
            PreyAvailabilityDET det = new PreyAvailabilityDET();
            ExcelPreyAvailabilityDET excel = new ExcelPreyAvailabilityDET(det);
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
            foreach (var dto in det.PreySurveys.Values)
            {
                Console.WriteLine("PreySurvey Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Prey.Values)
            {
                Console.WriteLine("Prey Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Sites.Values)
            {
                Console.WriteLine("Site Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Species.Values)
            {
                Console.WriteLine("Species Row: {0}, Value: {1}", count++, dto);
            }
            Console.WriteLine("Closing workbook named [{0}]", fName);
        }
        public static void WriteFile(string fName)
        {
            PreyAvailabilityDET det = new PreyAvailabilityDET();
            det.Id = Guid.NewGuid();
            det.Owner = "Dr. Frank N. Furter, ESQ";
            ExcelPreyAvailabilityDET excel = new ExcelPreyAvailabilityDET(det);
            CreateListOfPhonySurveys(det, 6);
            CreateListOfPhonyPrey(det, 6);
            CreateListOfPhonySpecies(det, 6);
            CreateListOfPhonySites(det, 4);
            //CreateListOfPhonyLifeStages(det, 6);
            excel.Save(fName);
        }

        private static void CreateListOfPhonySurveys(PreyAvailabilityDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonySurvey(count++);
                det.PreySurveys.Add(dto);
            }
        }

        private static PreySurveyDTO CreatePhonySurvey(int index)
        {
            PreySurveyDTO dto = new PreySurveyDTO();
            dto.SampleId = string.Format("SampleId {0}", index);
            dto.SiteId = string.Format("SiteId {0}", index);
            dto.InstrumentId = string.Format("InstrumentId {0}", index);
            dto.DateTime = Parsing.ParseDate(string.Format("{0}/01/2017", index));
            dto.SampleType = string.Format("SampleType {0}", index);
            dto.Comments = string.Format("Comments {0}", index);
            return dto;
        }

        private static void CreateListOfPhonyPrey(PreyAvailabilityDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyPrey(count++);
                det.Prey.Add(dto);
            }
        }

        private static PreyDTO CreatePhonyPrey(int index)
        {
            PreyDTO dto = new PreyDTO();
            dto.SampleId = string.Format("SampleId {0}", index);
            dto.SpeciesId = string.Format("SpeciesId {0}", index);
            dto.LifeStage = string.Format("LifeStage {0}", index);
            dto.Count = index;
            return dto;
        }

        //private static void CreateListOfPhonyLifeStages(PreyAvailabilityDET det, int numRows)
        //{
        //    int count = 1;
        //    for (int index = 0; index < numRows; index++)
        //    {
        //        var dto = PhonyLookup.CreatePhonyLifeStage(count++);
        //        det.LifeStages.Add(dto);
        //    }
        //}

        private static void CreateListOfPhonySpecies(PreyAvailabilityDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonySpecies(count++);
                det.Species.Add(dto);
            }
        }

        private static void CreateListOfPhonySites(PreyAvailabilityDET det, int numRows)
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
