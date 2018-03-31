using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class IdTagDTO : BaseDTO, IKeyed
    {
        public string FishId { get; set; }
        public string TagCode { get; set; }
        public string TagType { get; set; }
        public string TagManufacturer { get; set; }
        public string Comments { get; set; }

        public IdTagDTO()
        {
        }

        public IdTagDTO(Dictionary<string, string> values)
        {
            Schema schema = GetSchema();
            schema.ValidationIssues = ValidationIssues;
            FishId = (string)schema.Parse(values, "Fish ID");
            TagCode = (string)schema.Parse(values, "Tag Code");
            TagType = (string)schema.Parse(values, "Tag Type");
            TagManufacturer = (string)schema.Parse(values, "Tag Manuf");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "ID tag");
            schema.Add("Fish ID", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("Tag Code", typeof(string), SchemaEntryType.Normal, 1000, NullableType.IsNotNullable);
            schema.Add("Tag Type", typeof(string), SchemaEntryType.Normal, 1000, NullableType.IsNotNullable);
            schema.Add("Tag Manuf", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Fish ID", FishId);
            result.Add("Tag Code", TagCode);
            result.Add("Tag Type", TagType);
            result.Add("Tag Manuf", TagManufacturer);
            result.Add("Comments", Comments);
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(FishId, "Fish ID");
            schema.ValidateField(TagCode, "Tag Code");
            schema.ValidateField(TagType, "Tag Type");
            schema.ValidateField(TagManufacturer, "Tag Manuf");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey { get { return FishId + " " + TagCode; } }
    }
}
