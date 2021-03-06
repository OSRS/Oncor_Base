﻿using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs
{
    public class InstrumentDTO : BaseDTO, IKeyed
    {
        public string Key { get; set; }
        public string Name { get; set; }

        public InstrumentDTO()
        {
        }
        public InstrumentDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            Key = (string)schema.Parse(values, "Key");
            Name = (string)schema.Parse(values, "Name");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "instrument");
            schema.Add("Key", typeof(string), SchemaEntryType.LocalLookupKey, 1000);
            schema.Add("Name", typeof(string), SchemaEntryType.Normal, 1000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Key", Key);
            result.Add("Name", Name);
            return result;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(Key, "Key");
            schema.ValidateField(Name, "Name");
        }

        public string LookupKey => Key;
    }
}