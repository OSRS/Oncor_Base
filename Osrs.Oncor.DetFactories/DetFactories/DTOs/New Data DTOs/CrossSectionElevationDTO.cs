using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class CrossSectionElevationDTO : BaseDTO, IKeyed
    {
        public string SurveyId { get; set; }
        public double? DistanceFromOrigin { get; set; }
        public double? Elevation { get; set; }
        public string Comments { get; set; }

        public CrossSectionElevationDTO()
        {
        }

        public CrossSectionElevationDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            SurveyId = (string)schema.Parse(values, "SurveyId");
            DistanceFromOrigin = (double?)schema.Parse(values, "Distance from Origin");
            Elevation = (double?)schema.Parse(values, "Elevation");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("SurveyId", SurveyId);
            result.Add("Distance from Origin", FormatDouble(DistanceFromOrigin));
            result.Add("Elevation", FormatDouble(Elevation));
            result.Add("Comments", Comments);
            return result;
        }

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, "elevation");
            schema.Add("SurveyId", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("Distance from Origin", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DoubleRange(0.0, 5000.0));
            schema.Add("Elevation", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DoubleRange(-50000.0, 800000.0));
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(SurveyId, "SurveyId");
            schema.ValidateField(DistanceFromOrigin, "Distance from Origin");
            schema.ValidateField(Elevation, "Elevation");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => SurveyId;
    }
}