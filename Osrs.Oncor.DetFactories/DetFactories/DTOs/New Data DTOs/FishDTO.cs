using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class FishDTO : BaseDTO, IKeyed
    {
        public string FishId { get; set; }
        public string CatchId { get; set; }
        public string SpeciesId { get; set; }
        public double? LengthStandard { get; set; }
        public double? LengthFork { get; set; }
        public double? LengthTotal { get; set; }
        public double? Mass { get; set; }
        public bool? AdClipped { get; set; }
        public bool? CodedWireTag { get; set; }
        public string Comments { get; set; }

        public FishDTO()
        {
        }

        public FishDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            FishId = (string)schema.Parse(values, "Fish ID");
            CatchId = (string)schema.Parse(values, "CatchId");
            SpeciesId = (string)schema.Parse(values, "SpeciesId");
            LengthStandard = (double?)schema.Parse(values, "Length Standard");
            LengthFork = (double?)schema.Parse(values, "Length Fork");
            LengthTotal = (double?)schema.Parse(values, "Length Total");
            Mass = (double?)schema.Parse(values, "Mass");
            AdClipped = (bool?)schema.Parse(values, "AdClipped");
            CodedWireTag = (bool?)schema.Parse(values, "CWT");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "fish");
            schema.Add("Fish ID", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("CatchId", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("SpeciesId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("Length Standard", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable);
            schema.Add("Length Fork", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable);
            schema.Add("Length Total", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable);
            schema.Add("Mass", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable);
            schema.Add("AdClipped", typeof(bool), SchemaEntryType.Normal, 0, NullableType.IsNullable);
            schema.Add("CWT", typeof(bool), SchemaEntryType.Normal, 0, NullableType.IsNullable);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Fish ID", FishId);
            result.Add("CatchId", CatchId);
            result.Add("SpeciesId", SpeciesId);
            result.Add("Length Standard", FormatDouble(LengthStandard));
            result.Add("Length Fork", FormatDouble(LengthFork));
            result.Add("Length Total", FormatDouble(LengthTotal));
            result.Add("Mass", FormatDouble(Mass));
            result.Add("AdClipped", FormatBoolean(AdClipped));
            result.Add("CWT", FormatBoolean(CodedWireTag));
            result.Add("Comments", Comments);
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(FishId, "Fish ID");
            schema.ValidateField(CatchId, "CatchId");
            schema.ValidateField(SpeciesId, "SpeciesId");
            schema.ValidateField(LengthStandard, "Length Standard");
            schema.ValidateField(LengthFork, "Length Fork");
            schema.ValidateField(LengthTotal, "Length Total");
            schema.ValidateField(Mass, "Mass");
            schema.ValidateField(AdClipped, "AdClipped");
            schema.ValidateField(CodedWireTag, "CWT");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => FishId;
    }
}
