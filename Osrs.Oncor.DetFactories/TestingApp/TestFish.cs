using System;
using ExcelDETs.DETs;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace TestingApp
{
    static class TestFish
    {
        public static void ReadFile(string fName)
        {
            Console.WriteLine("reading workbook named [{0}]", fName);
            FishDET det = new FishDET();
            ExcelFishDET excel = new ExcelFishDET(det);
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
            foreach (var dto in det.CatchEfforts.Values)
            {
                Console.WriteLine("CatchEfforts Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.NetHaulEvents.Values)
            {
                Console.WriteLine("NetHaulEvents Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.FishCounts.Values)
            {
                Console.WriteLine("FishCounts Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.CatchMetrics.Values)
            {
                Console.WriteLine("CatchMetrics Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Fish.Values)
            {
                Console.WriteLine("Fish Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.IdTags.Values)
            {
                Console.WriteLine("IdTags Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Genetics.Values)
            {
                Console.WriteLine("Genetics Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Diet.Values)
            {
                Console.WriteLine("Diet Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.FishSpecies.Values)
            {
                Console.WriteLine("FishSpecies Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.MacroSpecies.Values)
            {
                Console.WriteLine("MacroSpecies Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Sites.Values)
            {
                Console.WriteLine("Site Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Nets.Values)
            {
                Console.WriteLine("Net Row: {0}, Value: {1}", count++, dto);
            }
            Console.WriteLine("Closing workbook named [{0}]", fName);
        }

        private static void CreateListOfPhonyCatchEfforts(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyCatchEffort(count++);
                det.CatchEfforts.Add(dto);
            }
        }
        public static CatchEffortDTO CreatePhonyCatchEffort(int count)
        {
            CatchEffortDTO dto = new CatchEffortDTO();
            dto.CatchId = String.Format("CatchId {0}", count);
            dto.SiteId = String.Format("SiteId {0}", count);
            dto.DateTime = Parsing.ParseDate(string.Format("{0}/01/2017", count)).Value;
            dto.CatchX = Parsing.ParseDouble("");
            dto.CatchY = Parsing.ParseDouble("");
            dto.CatchMethod = String.Format("CatchMethod {0}", count);
            dto.HabitatStrata = String.Format("HabitatStrata {0}", count);
            dto.Comments = String.Format("Comments {0}", count);
            dto.Depth = Parsing.ParseDouble("");
            dto.Temp = Parsing.ParseDouble("");
            dto.pH = Parsing.ParseDouble("");
            dto.DO = Parsing.ParseDouble("");
            dto.Salinity = Parsing.ParseDouble("");
            dto.Velocity = Parsing.ParseDouble("");
            return dto;
        }

        private static void CreateListOfPhonyNetHaulEvents(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyNetHaulEvent(count++);
                det.NetHaulEvents.Add(dto);
            }
        }

        public static NetHaulEventDTO CreatePhonyNetHaulEvent(int count)
        {
            NetHaulEventDTO dto = new NetHaulEventDTO();
            dto.CatchId = String.Format("CatchId {0}", count);
            dto.NetId = String.Format("NetId {0}", count);
            dto.AreaSampled = Parsing.ParseDouble("");
            dto.VolumeSampled = Parsing.ParseDouble("");
            return dto;
        }

        private static void CreateListOfPhonyFishCounts(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyFishCount(count++);
                det.FishCounts.Add(dto);
            }
        }
        public static FishCountDTO CreatePhonyFishCount(int count)
        {
            FishCountDTO dto = new FishCountDTO();
            dto.CatchId = String.Format("CatchId {0}", count);
            dto.SpeciesId = String.Format("SpeciesId {0}", count);
            dto.Count = (uint)count;
            return dto;
        }

        private static void CreateListOfPhonyCatchMetrics(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyCatchMetric(count++);
                det.CatchMetrics.Add(dto);
            }
        }

        public static CatchMetricDTO CreatePhonyCatchMetric(int count)
        {
            CatchMetricDTO dto = new CatchMetricDTO();
            dto.CatchId = String.Format("CatchId {0}", count);
            dto.Value = (double)count;
            dto.MetricType = String.Format("MetricType {0}", count);
            dto.Comments = String.Format("Comments {0}", count);
            return dto;
        }

        private static void CreateListOfPhonyFish(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyFish(count++);
                det.Fish.Add(dto);
            }
        }
        public static FishDTO CreatePhonyFish(int count)
        {
            FishDTO dto = new FishDTO();
            dto.FishId = String.Format("FishId {0}", count);
            dto.CatchId = String.Format("CatchId {0}", count);
            dto.SpeciesId = String.Format("SpeciesId {0}", count);
            dto.LengthStandard = Parsing.ParseDouble("");
            dto.LengthFork = Parsing.ParseDouble("");
            dto.LengthTotal = Parsing.ParseDouble("");
            dto.Mass = Parsing.ParseDouble("");
            dto.AdClipped = Parsing.ParseBoolean("True");
            dto.CodedWireTag = Parsing.ParseBoolean("True");
            return dto;
        }

        private static void CreateListOfPhonyIdTags(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyIdTag(count++);
                det.IdTags.Add(dto);
            }
        }

        public static IdTagDTO CreatePhonyIdTag(int count)
        {
            IdTagDTO dto = new IdTagDTO();
            dto.FishId = String.Format("FishId {0}", count);
            dto.TagCode = String.Format("TagCode {0}", count);
            dto.TagType = String.Format("TagType {0}", count);
            dto.TagManufacturer = String.Format("TagManufacturer {0}", count);
            return dto;
        }

        private static void CreateListOfPhonyGenetics(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyGenetic(count++);
                det.Genetics.Add(dto);
            }
        }
        public static GeneticDTO CreatePhonyGenetic(int count)
        {
            GeneticDTO dto = new GeneticDTO();
            dto.FishId = String.Format("FishId {0}", count);
            dto.GeneticSampleId = String.Format("GeneticSampleId {0}", count);
            dto.LabSampleId = String.Format("LabSampleId {0}", count);
            dto.BestStockEstimate = String.Format("BestStockEstimate {0}", count);
            dto.ProbabilityBest = Parsing.ParseDouble("");
            dto.SecondStockEstimate = String.Format("SecondStockEstimate {0}", count);
            dto.ProbabilitySecondBest = Parsing.ParseDouble("");
            dto.ThirdStockEstimate = String.Format("ThirdStockEstimate {0}", count);
            dto.ProbabilityThirdBest = Parsing.ParseDouble("");
            dto.Comments = String.Format("Comments {0}", count);
            return dto;
        }

        private static void CreateListOfPhonyDiet(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyDiet(count++);
                det.Diet.Add(dto);
            }
        }

        public static DietDTO CreatePhonyDiet(int count)
        {
            DietDTO dto = new DietDTO();
            dto.FishId = String.Format("FishId {0}", count);
            dto.GutSampleId = String.Format("GutSampleId {0}", count);
            dto.VialId = String.Format("VialId {0}", count);
            dto.SpeciesId = String.Format("SpeciesId {0}", count);
            dto.LifeStage = String.Format("LifeStage {0}", count);
            dto.Count = (uint)count;
            dto.SampleMass = Parsing.ParseDouble("");
            dto.WholeAnimalsWeighed = (uint)count;
            dto.IndividualMass = Parsing.ParseDouble("");
            dto.Comments = String.Format("Comments {0}", count);
            return dto;
        }

        private static void CreateListOfPhonyFishSpecies(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonyFishSpecies(count++);
                det.FishSpecies.Add(dto);
            }
        }

        private static void CreateListOfPhonyMacroSpecies(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonyMacroSpecies(count++);
                det.MacroSpecies.Add(dto);
            }
        }

        private static void CreateListOfPhonySites(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonySite(count++);
                det.Sites.Add(dto);
            }
        }

        private static void CreateListOfPhonyNets(FishDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonyNet(count++);
                det.Nets.Add(dto);
            }
        }

        public static void WriteFile(string fName)
        {
            FishDET det = new FishDET();
            det.Id = Guid.NewGuid();
            det.Owner = "Dr. Frank N. Furter, ESQ";
            ExcelFishDET excel = new ExcelFishDET(det);
            CreateListOfPhonyCatchEfforts(det, 4);
            CreateListOfPhonyNetHaulEvents(det, 6);
            CreateListOfPhonyFishCounts(det, 4);
            CreateListOfPhonyCatchMetrics(det, 6);
            CreateListOfPhonyFish(det, 4);
            CreateListOfPhonyIdTags(det, 6);
            CreateListOfPhonyGenetics(det, 4);
            CreateListOfPhonyDiet(det, 6);
            CreateListOfPhonyFishSpecies(det, 4);
            CreateListOfPhonyMacroSpecies(det, 6);
            CreateListOfPhonySites(det, 4);
            CreateListOfPhonyNets(det, 6);
            excel.Save(fName);
        }
    }
}
