using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs
{
    public class SensorDTO : BaseDTO, IKeyed
    {
        public string Key { get; set; }
        public string Name { get; set; }
        //public string Model { get; set; }
        //public string InstrumentType { get; set; }
        //public string InstrumentClass { get; set; }
        //public string Manufacturer { get; set; }
        //public string SensorList { get; set; }

        public SensorDTO()
        {
        }

        public SensorDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            Key = (string)schema.Parse(values, "Key");
            Name = (string)schema.Parse(values, "Name");
            //Model = (string)schema.Parse(values, "Model");
            //InstrumentType = (string)schema.Parse(values, "InstrumentType");
            //InstrumentClass = (string)schema.Parse(values, "InstrumentClass");
            //Manufacturer = (string)schema.Parse(values, "Manufacturer");
            //SensorList = (string)schema.Parse(values, "SensorList");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "sensor");
            schema.Add("Key", typeof(string), SchemaEntryType.LocalLookupKey, 1000);
            schema.Add("Name", typeof(string), SchemaEntryType.Normal, 1000);
            //schema.Add("Model", typeof(string), SchemaEntryType.Normal, 1000);
            //schema.Add("InstrumentType", typeof(string), SchemaEntryType.Normal, 1000);
            //schema.Add("InstrumentClass", typeof(string), SchemaEntryType.Normal, 1000);
            //schema.Add("Manufacturer", typeof(string), SchemaEntryType.Normal, 1000);
            //schema.Add("SensorList", typeof(string), SchemaEntryType.Normal, 1000);
            return schema;
        }


        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Key", Key);
            result.Add("Name", Name);
            //result.Add("Model", Model);
            //result.Add("InstrumentType", InstrumentType);
            //result.Add("InstrumentClass", InstrumentClass);
            //result.Add("Manufacturer", Manufacturer);
            //result.Add("SensorList", SensorList);
            return result;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(Key, "Key");
            schema.ValidateField(Name, "Name");
            //schema.ValidateField(Model, "Model");
            //schema.ValidateField(InstrumentType, "InstrumentType");
            //schema.ValidateField(InstrumentClass, "InstrumentClass");
            //schema.ValidateField(Manufacturer, "Manufacturer");
            //schema.ValidateField(SensorList, "SensorList");
        }

        public string LookupKey => Key;
    }
}
