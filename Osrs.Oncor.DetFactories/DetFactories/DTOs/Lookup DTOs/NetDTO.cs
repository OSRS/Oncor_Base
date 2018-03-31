using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs
{
    public class NetDTO : BaseDTO, IKeyed
    {
        public string Key { get; set; }
        public string Name { get; set; }
        //public string Type { get; set; }
        //public string Details { get; set; }
        //public string Description { get; set; }

        public NetDTO()
        {
        }

        public NetDTO(Dictionary<string, string> values)
        {
            Schema schema = GetSchema();
            schema.ValidationIssues = ValidationIssues;
            Key = (string)schema.Parse(values, "Key");
            Name = (string)schema.Parse(values, "Name");
            //Type = (string)schema.Parse(values, "Type");
            //Details = (string)schema.Parse(values, "Details");
            //Description = (string)schema.Parse(values, "Description");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "net");
            schema.Add("Key", typeof(string), SchemaEntryType.LocalLookupKey, 1000);
            schema.Add("Name", typeof(string), SchemaEntryType.Normal, 1000);
            //schema.Add("Type", typeof(string), SchemaEntryType.Normal, 1000);
            //schema.Add("Details", typeof(string), SchemaEntryType.Normal, 1000);
            //schema.Add("Description", typeof(string), SchemaEntryType.Normal, 1000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Key", Key);
            result.Add("Name", Name);
            //result.Add("Type", Type);
            //result.Add("Details", Details);
            //result.Add("Description", Description);
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(Key, "Key");
            schema.ValidateField(Name, "Name");
            //schema.ValidateField(Type, "Type");
            //schema.ValidateField(Details, "Details");
            //schema.ValidateField(Description, "Description");
        }

        public string LookupKey => Key;
    }
}
