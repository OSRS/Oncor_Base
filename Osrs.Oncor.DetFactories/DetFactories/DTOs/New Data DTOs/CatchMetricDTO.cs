using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class CatchMetricDTO : BaseDTO, IKeyed
    {
        public string CatchId { get; set; }
        public double? Value { get; set; }
        public string MetricType { get; set; }
        public string Comments { get; set; }

        public CatchMetricDTO()
        {
        }

        public CatchMetricDTO(Dictionary<string, string> values)
        {
            Schema schema = GetSchema();
            schema.ValidationIssues = ValidationIssues;
            CatchId = (string)schema.Parse(values, "CatchId");
            Value = (double?)schema.Parse(values, "Value");
            MetricType = (string)schema.Parse(values, "Metric Type");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "catch metric");
            schema.Add("CatchId", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("Value", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable);
            schema.Add("Metric Type", typeof(string), SchemaEntryType.Normal, 1000, NullableType.IsNotNullable);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("CatchId", CatchId);
            result.Add("Value", FormatDouble(Value));
            result.Add("Metric Type", MetricType);
            result.Add("Comments", Comments);
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(CatchId, "CatchId");
            schema.ValidateField(Value, "Value");
            schema.ValidateField(MetricType, "Metric Type");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey { get { return CatchId + " " + MetricType; } }
    }
}
