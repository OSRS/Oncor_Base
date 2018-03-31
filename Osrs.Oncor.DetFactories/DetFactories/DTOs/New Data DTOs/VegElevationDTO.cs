using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class VegSurveyDTO : BaseDTO, IKeyed
    {
        private const string dtoName = "survey";
        public string SurveyId { get; set; }
        public string SiteId { get; set; }

        public string PlotTypeId { get; set; }

        public double? Area { get; set; }

        public double? AdHocLat { get; set; }
        public double? AdHocLon { get; set; }

        public double? MinElevation { get; set; }
        public double? MaxElevation { get; set; }
        public string Comments { get; set; }

        public VegSurveyDTO()
        { }

        public VegSurveyDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            SurveyId = (string)schema.Parse(values, "SurveyId");
            SiteId = (string)schema.Parse(values, "SiteId");
            PlotTypeId = (string)schema.Parse(values, "PlotTypeId");
            Area = (double?)schema.Parse(values, "Area");
            AdHocLat = (double?)schema.Parse(values, "AdHocLat");
            AdHocLon = (double?)schema.Parse(values, "AdHocLon");
            MinElevation = (double?)schema.Parse(values, "MinElevation");
            MaxElevation = (double?)schema.Parse(values, "MaxElevation");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, dtoName);
            schema.Add("SurveyId", typeof(string), SchemaEntryType.LocalMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("SiteId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("PlotTypeId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNullable);
            schema.Add("Area", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, Double.MaxValue));
            schema.Add("MinElevation", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, Double.MaxValue));
            schema.Add("MaxElevation", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, Double.MaxValue));
            schema.Add("AdHocLat", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(-90, 90.0));
            schema.Add("AdHocLon", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(-180.0, 180.0));
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000, NullableType.IsNullable);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("SurveyId", SurveyId);
            result.Add("SiteId", SiteId);
            result.Add("PlotTypeId", PlotTypeId);
            result.Add("Area", FormatDouble(Area));
            result.Add("MinElevation", FormatDouble(MinElevation));
            result.Add("MaxElevation", FormatDouble(MaxElevation));
            result.Add("AdHocLat", FormatDouble(AdHocLat));
            result.Add("AdHocLon", FormatDouble(AdHocLon));
            result.Add("Comments", FormatString(Comments));
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(SurveyId, "SurveyId");
            schema.ValidateField(SiteId, "SiteId");
            schema.ValidateField(PlotTypeId, "PlotTypeId");
            schema.ValidateField(Area, "Area");
            schema.ValidateField(AdHocLat, "AdHocLat");
            schema.ValidateField(AdHocLon, "AdHocLon");
            schema.ValidateField(MinElevation, "MinElevation");
            schema.ValidateField(MaxElevation, "MaxElevation");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey => string.Format("{0} {1} {2} {3}", SurveyId, FormatString(SiteId), FormatDouble(AdHocLat), FormatDouble(AdHocLon));
    }

    public class VegElevationDTO : VegDataDTO, IKeyed
    {
        private const string dtoName = "elevation";
        protected override string DtoName { get { return dtoName; } }

        public double? MinElevation { get; set; }
        public double? MaxElevation { get; set; }

        public VegElevationDTO():base()
        {
        }

        public VegElevationDTO(Dictionary<string, string> values):base(values)
        {
            MinElevation = (double?)schema.Parse(values, "MinElevation");
            MaxElevation = (double?)schema.Parse(values, "MaxElevation");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = BaseSchema(dtoName);
            schema.Add("MinElevation", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, Double.MaxValue));
            schema.Add("MaxElevation", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, Double.MaxValue));
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = base.Values();
            result.Add("MinElevation", FormatDouble(MinElevation));
            result.Add("MaxElevation", FormatDouble(MaxElevation));
            return result;
        }
        public override void Validate()
        {
            Schema schema = PreValidate();
            schema.ValidateField(MinElevation, "MinElevation");
            schema.ValidateField(MaxElevation, "MaxElevation");
            bool[] isPresent = new[]
            {
                MinElevation.HasValue, MaxElevation.HasValue
            };
            schema.ValidateMinimumOptionalFields(dtoName, isPresent, 1);
        }
    }
}
