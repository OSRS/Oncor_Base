using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class DietDTO : BaseDTO, IKeyed
    {
        public string FishId { get; set; }
        public string GutSampleId { get; set; }
        public string VialId { get; set; }
        public string SpeciesId { get; set; }
        public string LifeStage { get; set; }
        public uint? Count { get; set; }
        public double? SampleMass { get; set; }
        public uint? WholeAnimalsWeighed { get; set; }
        public double? IndividualMass { get; set; }
        public string Comments { get; set; }

        public DietDTO()
        {
        }

        public DietDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            FishId = (string)schema.Parse(values, "Fish ID");
            GutSampleId = (string)schema.Parse(values, "Gut Sample ID");
            VialId = (string)schema.Parse(values, "Vial ID");
            SpeciesId = (string)schema.Parse(values, "Species ID");
            LifeStage = (string)schema.Parse(values, "Life Stage");
            Count = (uint?)schema.Parse(values, "Count");
            SampleMass = (double?)schema.Parse(values, "Sample Mass");
            WholeAnimalsWeighed = (uint?)schema.Parse(values, "Whole Animals Weighed");
            IndividualMass = (double?)schema.Parse(values, "Individual Mass");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "diet");
            schema.Add("Fish ID", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("Gut Sample ID", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("Vial ID", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Species ID", typeof(string), SchemaEntryType.ForeignLookupKey, 1000);
            schema.Add("Life Stage", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Count", typeof(uint), SchemaEntryType.Normal, 0, NullableType.IsNotNullable, new UIntRange(0, UInt32.MaxValue));
            schema.Add("Sample Mass", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, Double.MaxValue));
            schema.Add("Whole Animals Weighed", typeof(uint), SchemaEntryType.Normal, 0, NullableType.IsNullable, new UIntRange(0, uint.MaxValue));
            schema.Add("Individual Mass", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, Double.MaxValue));
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Fish ID", FishId);
            result.Add("Gut Sample ID", GutSampleId);
            result.Add("Vial ID", VialId);
            result.Add("Species ID", SpeciesId);
            result.Add("Life Stage", LifeStage);
            result.Add("Count", FormatUnsignedInteger(Count));
            result.Add("Sample Mass", FormatDouble(SampleMass));
            result.Add("Whole Animals Weighed", FormatUnsignedInteger(WholeAnimalsWeighed));
            result.Add("Individual Mass", FormatDouble(IndividualMass));
            result.Add("Comments", Comments);
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(FishId, "Fish ID");
            schema.ValidateField(GutSampleId, "Gut Sample ID");
            schema.ValidateField(VialId, "Vial ID");
            schema.ValidateField(SpeciesId, "Species ID");
            schema.ValidateField(LifeStage, "Life Stage");
            schema.ValidateField(Count, "Count");
            schema.ValidateField(SampleMass, "Sample Mass");
            schema.ValidateField(WholeAnimalsWeighed, "Whole Animals Weighed");
            schema.ValidateField(IndividualMass, "Individual Mass");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => string.Format("{0} {1} {2} {3}", FishId, GutSampleId, VialId, SpeciesId);
    }
}
