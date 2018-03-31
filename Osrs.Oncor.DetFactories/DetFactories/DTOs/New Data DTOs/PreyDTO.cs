using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class PreyDTO : BaseDTO, IKeyed
    {
        public string SampleId { get; set; }
        public string SpeciesId { get; set; }
        public string LifeStage { get; set; }
        public int? Count { get; set; }

        public PreyDTO()
        {
        }

        public PreyDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            SampleId = (string)schema.Parse(values, "SampleId");
            SpeciesId = (string)schema.Parse(values, "SpeciesId");
            LifeStage = (string)schema.Parse(values, "Life Stage");
            Count = (int?)schema.Parse(values, "Count");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, "prey");
            schema.Add("SampleId", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000);
            schema.Add("SpeciesId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000);
            schema.Add("Life Stage", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Count", typeof(int), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new DoubleRange(0.0, (double)Int32.MaxValue));
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("SampleId", SampleId);
            result.Add("SpeciesId", SpeciesId);
            result.Add("Life Stage", LifeStage);
            result.Add("Count", FormatInteger(Count));
            return result;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(SampleId, "SampleId");
            schema.ValidateField(SpeciesId, "SpeciesId");
            schema.ValidateField(LifeStage, "Life Stage");
            schema.ValidateField(Count, "Count");
        }

        public string LookupKey => SampleId;
    }
}