using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class SedimentAccretionElevation : BaseDTO, IKeyed
    {
        public string SurveyId { get; set; }
        public double? VertCmDown { get; set; }
        public double? HorizCmFromA { get; set; }
        public string Comments { get; set; }

        public SedimentAccretionElevation()
        {
        }

        public SedimentAccretionElevation(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            SurveyId = (string)schema.Parse(values, "SurveyId");
            VertCmDown = (double?)schema.Parse(values, "VertCmDown");
            HorizCmFromA = (double?)schema.Parse(values, "HorizCmFromA");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("SurveyId", SurveyId);
            result.Add("VertCmDown", FormatDouble(VertCmDown));
            result.Add("HorizCmFromA", FormatDouble(HorizCmFromA));
            result.Add("Comments", Comments);
            return result;
        }

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, "elevation");
            schema.Add("SurveyId", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("VertCmDown", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DoubleRange(0.0, Double.MaxValue));
            schema.Add("HorizCmFromA", typeof(double), SchemaEntryType.Normal);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(SurveyId, "SurveyId");
            schema.ValidateField(VertCmDown, "VertCmDown");
            schema.ValidateField(HorizCmFromA, "HorizCmFromA");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => SurveyId;
    }
}