using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class PreySurveyDTO : BaseDTO, IKeyed
    {
        public string SampleId { get; set; }
        public string SiteId { get; set; }
        public string InstrumentId { get; set; }
        public DateTime? DateTime { get; set; }
        public string SampleType { get; set; }
        public string Comments { get; set; }

        public PreySurveyDTO()
        {
        }

        public PreySurveyDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            SampleId = (string)schema.Parse(values, "SampleId");
            SiteId = (string)schema.Parse(values, "SiteId");
            InstrumentId = (string)schema.Parse(values, "InstrumentId");
            DateTime = (DateTime?)schema.Parse(values, "DateTime");
            SampleType = (string)schema.Parse(values, "SampleType");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, "prey survey");
            schema.Add("SampleId", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000);
            schema.Add("SiteId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000);
            schema.Add("InstrumentId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000);
            schema.Add("DateTime", typeof(DateTime), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DateRange(new DateTime(1900, 1, 1), null));
            schema.Add("SampleType", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("SampleId", SampleId);
            result.Add("SiteId", SiteId);
            result.Add("InstrumentId", InstrumentId);
            result.Add("DateTime", FormatDate(DateTime));
            result.Add("SampleType", SampleType);
            result.Add("Comments", Comments);
            return result;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(SampleId, "SampleId");
            schema.ValidateField(SiteId, "SiteId");
            schema.ValidateField(InstrumentId, "InstrumentId");
            schema.ValidateField(DateTime, "DateTime");
            schema.ValidateField(SampleType, "SampleType");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => SampleId;
    }
}