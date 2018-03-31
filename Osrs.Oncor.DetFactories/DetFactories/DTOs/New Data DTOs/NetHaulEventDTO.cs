using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class NetHaulEventDTO : BaseDTO, IKeyed
    {
        public string CatchId { get; set; }
        public string NetId { get; set; }
        public double? AreaSampled { get; set; }
        public double? VolumeSampled { get; set; }
        public string Comments { get; set; }

        public NetHaulEventDTO()
        {
        }

        public NetHaulEventDTO(Dictionary<string, string> values)
        {
            Schema schema = GetSchema();
            schema.ValidationIssues = ValidationIssues;
            CatchId = (string)schema.Parse(values, "CatchId");
            NetId = (string)schema.Parse(values, "NetId");
            AreaSampled = (double?)schema.Parse(values, "Area Sampled");
            VolumeSampled = (double?)schema.Parse(values, "Volume Sampled");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "net haul");
            schema.Add("CatchId", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("NetId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("Area Sampled", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, Double.MaxValue));
            schema.Add("Volume Sampled", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, Double.MaxValue));
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("CatchId", CatchId);
            result.Add("NetId", NetId);
            result.Add("Area Sampled", FormatDouble(AreaSampled));
            result.Add("Volume Sampled", FormatDouble(VolumeSampled));
            result.Add("Comments", Comments);
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(CatchId, "CatchId");
            schema.ValidateField(NetId, "NetId");
            schema.ValidateField(AreaSampled, "Area Sampled");
            schema.ValidateField(VolumeSampled, "Volume Sampled");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => string.Format("{0} {1} {2}", CatchId, NetId, Comments);
    }
}
