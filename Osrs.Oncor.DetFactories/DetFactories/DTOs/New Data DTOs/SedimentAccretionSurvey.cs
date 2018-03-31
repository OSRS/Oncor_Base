using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class SedimentAccretionSurvey : BaseDTO, IKeyed
    {
        private const string dtoName = "survey";
        public string SurveyId { get; set; }
        public string SiteId { get; set; }
        public DateTime? DateTime { get; set; }
        public double? ElevTopA { get; set; }
        public double? ElevTopB { get; set; }
        public string Comments { get; set; }

        public SedimentAccretionSurvey()
        {
        }

        public SedimentAccretionSurvey(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            SurveyId = (string)schema.Parse(values, "Survey ID");
            SiteId = (string)schema.Parse(values, "Site ID");
            DateTime = (DateTime?)schema.Parse(values, "DateTime");
            ElevTopA = (double?)schema.Parse(values, "ElevTopA");
            ElevTopB = (double?)schema.Parse(values, "ElevTopB");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, "survey");
            schema.Add("Survey ID", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000);
            schema.Add("Site ID", typeof(string), SchemaEntryType.ForeignLookupKey, 1000);
            schema.Add("DateTime", typeof(DateTime), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DateRange(new DateTime(1900, 1, 1), null));
            schema.Add("ElevTopA", typeof(double?), SchemaEntryType.Normal);
            schema.Add("ElevTopB", typeof(double?), SchemaEntryType.Normal);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Survey ID", SurveyId);
            result.Add("Site ID", SiteId);
            result.Add("DateTime", FormatDate(DateTime));
            result.Add("ElevTopA", FormatDouble(ElevTopA));
            result.Add("ElevTopB", FormatDouble(ElevTopB));
            result.Add("Comments", Comments);
            return result;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(SurveyId, "Survey ID");
            schema.ValidateField(SiteId, "Site ID");
            schema.ValidateField(DateTime, "DateTime");
            schema.ValidateField(ElevTopA, "ElevTopA");
            schema.ValidateField(ElevTopB, "ElevTopB");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => SurveyId;
    }
}