using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class CrossSectionSurveyDTO : BaseDTO, IKeyed
    {
        private const string dtoName = "survey";
        public string SurveyId { get; set; }
        public string SiteId { get; set; }
        public string InstrumentId { get; set; }
        public DateTime? DateTime { get; set; }
        public double? OriginX { get; set; }
        public double? OriginY { get; set; }
        public double? DestinationX { get; set; }
        public double? DestinationY { get; set; }
        public string Comments { get; set; }

        public CrossSectionSurveyDTO()
        {
        }

        public CrossSectionSurveyDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            SurveyId = (string)schema.Parse(values, "Survey ID");
            SiteId = (string)schema.Parse(values, "Site ID");
            InstrumentId = (string)schema.Parse(values, "Instrument ID");
            DateTime = (DateTime)schema.Parse(values, "DateTime");
            OriginX = (double?)schema.Parse(values, "Origin X");
            OriginY = (double?)schema.Parse(values, "Origin Y");
            DestinationX = (double?)schema.Parse(values, "Destination X");
            DestinationY = (double?)schema.Parse(values, "Destination Y");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, "survey");
            schema.Add("Survey ID", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000);
            schema.Add("Site ID", typeof(string), SchemaEntryType.ForeignLookupKey, 1000);
            schema.Add("Instrument ID", typeof(string), SchemaEntryType.ForeignLookupKey, 1000);
            schema.Add("DateTime", typeof(DateTime), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DateRange(new DateTime(1900, 1, 1), null));
            schema.Add("Origin X", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DoubleRange(-180.0, 180.0));
            schema.Add("Origin Y", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DoubleRange(-90.0, 90.0));
            schema.Add("Destination X", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DoubleRange(-180.0, 180.0));
            schema.Add("Destination Y", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DoubleRange(-90.0, 90.0));
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Survey ID", SurveyId);
            result.Add("Site ID", SiteId);
            result.Add("Instrument ID", InstrumentId);
            result.Add("DateTime", FormatDate(DateTime));
            result.Add("Origin X", FormatDouble(OriginX));
            result.Add("Origin Y", FormatDouble(OriginY));
            result.Add("Destination X", FormatDouble(DestinationX));
            result.Add("Destination Y", FormatDouble(DestinationY));
            result.Add("Comments", Comments);
            return result;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(SurveyId, "Survey ID");
            schema.ValidateField(SiteId, "Site ID");
            schema.ValidateField(InstrumentId, "Instrument ID");
            schema.ValidateField(DateTime, "DateTime");
            schema.ValidateField(OriginX, "Origin X");
            schema.ValidateField(OriginY, "Origin Y");
            schema.ValidateField(DestinationX, "Destination X");
            schema.ValidateField(DestinationY, "Destination Y");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => SurveyId;
    }
}