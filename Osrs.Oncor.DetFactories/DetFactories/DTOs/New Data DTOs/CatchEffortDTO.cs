using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class CatchEffortDTO : BaseDTO, IKeyed
    {
        private const string dtoName = "catch effort";
        public string CatchId { get; set; }
        public string SiteId { get; set; }
        public DateTime DateTime { get; set; }
        public double? CatchX { get; set; }
        public double? CatchY { get; set; }
        public string CatchMethod { get; set; }
        public string HabitatStrata { get; set; }
        public string Comments { get; set; }
        public double? Depth { get; set; }
        public double? Temp { get; set; }
        public double? pH { get; set; }
        public double? DO { get; set; }
        public double? Salinity { get; set; }
        public double? Velocity { get; set; }

        public CatchEffortDTO()
        {
        }

        public CatchEffortDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            CatchId = (string)schema.Parse(values, "CatchId");
            SiteId = (string)schema.Parse(values, "SiteId");
            DateTime = (DateTime)schema.Parse(values, "DateTime");
            CatchX = (double?)schema.Parse(values, "Catch X");
            CatchY = (double?)schema.Parse(values, "Catch Y");
            CatchMethod = (string)schema.Parse(values, "CatchMethod");
            HabitatStrata = (string)schema.Parse(values, "HabitatStrata");
            Comments = (string)schema.Parse(values, "Comments");
            Depth = (double?)schema.Parse(values, "Depth");
            Temp = (double?)schema.Parse(values, "Temp");
            pH = (double?)schema.Parse(values, "pH");
            DO = (double?)schema.Parse(values, "DO");
            Salinity = (double?)schema.Parse(values, "Salinity");
            Velocity = (double?)schema.Parse(values, "Velocity");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, dtoName);
            schema.Add("CatchId", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("SiteId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("DateTime", typeof(DateTime), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DateRange(new DateTime(1900, 1, 1), null));
            schema.Add("Catch X", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(-180.0, 180.0));
            schema.Add("Catch Y", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(-90.0, 90.0));
            schema.Add("CatchMethod", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("HabitatStrata", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            schema.Add("Depth", typeof(double), SchemaEntryType.Normal);
            schema.Add("Temp", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(-273.15, 100.0));
            schema.Add("pH", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(1.0, 14.0));
            schema.Add("DO", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 30.0));
            schema.Add("Salinity", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 40.0));
            schema.Add("Velocity", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 6.0));
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("CatchId", CatchId);
            result.Add("SiteId", SiteId);
            result.Add("DateTime", FormatDate(DateTime));
            result.Add("Catch X", FormatDouble(CatchX));
            result.Add("Catch Y", FormatDouble(CatchY));
            result.Add("CatchMethod", CatchMethod);
            result.Add("HabitatStrata", HabitatStrata);
            result.Add("Comments", Comments);
            result.Add("Depth", FormatDouble(Depth));
            result.Add("Temp", FormatDouble(Temp));
            result.Add("pH", FormatDouble(pH));
            result.Add("DO", FormatDouble(DO));
            result.Add("Salinity", FormatDouble(Salinity));
            result.Add("Velocity", FormatDouble(Velocity));
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(CatchId, "CatchId");
            schema.ValidateField(SiteId, "SiteId");
            schema.ValidateField(CatchMethod, "CatchMethod");
            schema.ValidateField(DateTime, "DateTime");
            schema.ValidateField(CatchX, "Catch X");
            schema.ValidateField(CatchY, "Catch Y");
            schema.ValidateField(CatchMethod, "CatchMethod");
            schema.ValidateField(HabitatStrata, "HabitatStrata");
            schema.ValidateField(Comments, "Comments");
            schema.ValidateField(Depth, "Depth");
            schema.ValidateField(Temp, "Temp");
            schema.ValidateField(pH, "pH");
            schema.ValidateField(DO, "DO");
            schema.ValidateField(Salinity, "Salinity");
            schema.ValidateField(Velocity, "Velocity");
        }

        public string LookupKey => CatchId;
    }
}
