using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class DeploymentDTO : BaseDTO, IKeyed
    {
        public string DeployCode { get; set; }
        public string SiteId { get; set; }
        public string InstrumentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Comments { get; set; }

        public DeploymentDTO()
        {
        }

        public DeploymentDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            DeployCode = (string)schema.Parse(values, "DeployCode");
            SiteId = (string)schema.Parse(values, "SiteId");
            InstrumentId = (string)schema.Parse(values, "InstrumentId");
            StartDate = (DateTime?)schema.Parse(values, "StartDate");
            EndDate = (DateTime?)schema.Parse(values, "EndDate");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, "deployment");
            schema.Add("DeployCode", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("SiteId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("InstrumentId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("StartDate", typeof(DateTime), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DateRange(new DateTime(1900, 1, 1), null));
            schema.Add("EndDate", typeof(DateTime), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DateRange(new DateTime(1900, 1, 1), null));
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000, NullableType.IsNullable);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("DeployCode", DeployCode);
            result.Add("SiteId", SiteId);
            result.Add("InstrumentId", InstrumentId);
            result.Add("StartDate", FormatDate(StartDate));
            result.Add("EndDate", FormatDate(EndDate));
            result.Add("Comments", Comments);
            return result;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(DeployCode, "DeployCode");
            schema.ValidateField(SiteId, "SiteId");
            schema.ValidateField(InstrumentId, "InstrumentId");
            schema.ValidateField(StartDate, "StartDate");
            schema.ValidateField(EndDate, "EndDate");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => DeployCode;
    }
}
