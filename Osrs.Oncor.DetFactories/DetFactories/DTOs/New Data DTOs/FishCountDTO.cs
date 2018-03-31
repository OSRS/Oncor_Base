using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class FishCountDTO : BaseDTO, IKeyed
    {
        public string CatchId { get; set; }
        public string SpeciesId { get; set; }
        public uint? Count { get; set; }
        public string Comments { get; set; }

        public FishCountDTO()
        {
        }

        public FishCountDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            CatchId = (string)schema.Parse(values, "CatchId");
            SpeciesId = (string)schema.Parse(values, "SpeciesId");
            Count = (uint?)schema.Parse(values, "Count");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "fish count");
            schema.Add("CatchId", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("SpeciesId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("Count", typeof(uint), SchemaEntryType.Normal, 0, NullableType.IsNotNullable);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("CatchId", CatchId);
            result.Add("SpeciesId", SpeciesId);
            result.Add("Count", FormatUnsignedInteger(Count));
            result.Add("Comments", Comments);
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(CatchId, "CatchId");
            schema.ValidateField(SpeciesId, "SpeciesId");
            schema.ValidateField(Count, "Count");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => string.Format("{0} {1}", CatchId, SpeciesId);
    }
}
