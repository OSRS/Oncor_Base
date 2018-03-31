using System;
using ExcelDETs.DETs;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace TestingApp
{
    static class TestWaterQuality
    {
        public static void ReadFile(string fName)
        {
            Console.WriteLine("reading workbook named [{0}]", fName);
            WaterQualityDET det = new WaterQualityDET();
            ExcelWaterQualityDET excel = new ExcelWaterQualityDET(det);
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
            foreach (var dto in det.Deployments.Values)
            {
                Console.WriteLine("Deployment Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Measurements.Values)
            {
                Console.WriteLine("Measurement Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Sites.Values)
            {
                Console.WriteLine("Site Row: {0}, Value: {1}", count++, dto);
            }
            count = 1;
            foreach (var dto in det.Instruments.Values)
            {
                Console.WriteLine("Sensor Row: {0}, Value: {1}", count++, dto);
            }
            Console.WriteLine("Closing workbook named [{0}]", fName);
        }

        public static void WriteFile(string fName)
        {
            WaterQualityDET det = new WaterQualityDET();
            det.Id = Guid.NewGuid();
            det.Owner = "Dr. Frank N. Furter, ESQ";
            ExcelWaterQualityDET excel = new ExcelWaterQualityDET(det);
            CreateListOfPhonyDeployments(det, 4);
            CreateListOfPhonyMeasurements(det, 6);
            CreateListOfPhonySites(det, 4);
            CreateListOfPhonySensors(det, 6);
            excel.Save(fName);
        }

        private static void CreateListOfPhonyMeasurements(WaterQualityDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyMeasurement(count++);
                det.Measurements.Add(dto);
            }
        }

        private static MeasurementDTO CreatePhonyMeasurement(int index)
        {
            MeasurementDTO dto = new MeasurementDTO();
            dto.DeployCode = string.Format("DeployCode {0}", index);
            dto.MeasureDateTime = Parsing.ParseDate(string.Format("{0}/01/2017", index));
            dto.Conductivity = Parsing.ParseDouble("");
            dto.DO = Parsing.ParseDouble("");
            dto.Salinity = Parsing.ParseDouble("");
            dto.SurfaceElevation = Parsing.ParseDouble("23.0");
            dto.Temperature = Parsing.ParseDouble("");
            dto.Velocity = Parsing.ParseDouble("");
            dto.pH = Parsing.ParseDouble("");
            return dto;
        }

        private static void CreateListOfPhonyDeployments(WaterQualityDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = CreatePhonyDeployment(count++);
                det.Deployments.Add(dto);
            }
        }

        private static DeploymentDTO CreatePhonyDeployment(int index)
        {
            DeploymentDTO dto = new DeploymentDTO();
            dto.DeployCode = string.Format("DeployCode {0}", index);
            dto.SiteId = string.Format("SiteID {0}", index);
            dto.InstrumentId = string.Format("SensorID {0}", index);
            dto.StartDate = Parsing.ParseDate(string.Format("{0}/01/2017", index));
            dto.EndDate = Parsing.ParseDate(string.Format("{0}/15/2017", index));
            dto.Comments = "No comment.";
            return dto;
        }

        private static void CreateListOfPhonySites(WaterQualityDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var dto = PhonyLookup.CreatePhonySite(count++);
                det.Sites.Add(dto);
            }
        }

        private static void CreateListOfPhonySensors(WaterQualityDET det, int numRows)
        {
            int count = 1;
            for (int index = 0; index < numRows; index++)
            {
                var s = PhonyLookup.CreatePhonySensor(count++);
                det.Instruments.Add(s);
            }
        }
    }
}
