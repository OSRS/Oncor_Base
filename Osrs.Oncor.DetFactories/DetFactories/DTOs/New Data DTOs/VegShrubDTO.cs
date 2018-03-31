using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public abstract class VegDataDTO : BaseDTO, IKeyed
    {
        protected Schema schema;
        protected abstract string DtoName { get; }

        public string SurveyId { get; set; }
        public string SiteId { get; set; }

        public DateTime? MeasureDateTime { get; set; }

        public double? AdHocLat { get; set; }
        public double? AdHocLon { get; set; }

        protected VegDataDTO()
        { }

        protected VegDataDTO(Dictionary<string, string> values)
        {
            schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            SurveyId = (string)schema.Parse(values, "SurveyId");
            SiteId = (string)schema.Parse(values, "SiteId");
            MeasureDateTime = (DateTime?)schema.Parse(values, "DateTime");
            AdHocLat = (double?)schema.Parse(values, "AdHocLat");
            AdHocLon = (double?)schema.Parse(values, "AdHocLon");
        }

        protected static Schema BaseSchema(string name)
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, name);
            schema.Add("SurveyId", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("SiteId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNullable);
            schema.Add("DateTime", typeof(DateTime), SchemaEntryType.Normal, 0, NullableType.IsNotNullable);
            schema.Add("AdHocLat", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(-90, 90.0));
            schema.Add("AdHocLon", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(-180.0, 180.0));
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("SurveyId", SurveyId);
            result.Add("SiteId", SiteId);
            result.Add("DateTime", FormatDate(MeasureDateTime));
            result.Add("AdHocLat", FormatDouble(AdHocLat));
            result.Add("AdHocLon", FormatDouble(AdHocLon));
            return result;
        }
        protected Schema PreValidate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(SurveyId, "SurveyId");
            schema.ValidateField(SiteId, "SiteId");
            schema.ValidateField(MeasureDateTime, "DateTime");
            schema.ValidateField(AdHocLat, "AdHocLat");
            schema.ValidateField(AdHocLon, "AdHocLon");
            bool[] isPresent = new[]
            {
                !string.IsNullOrEmpty(SiteId), AdHocLat.HasValue && AdHocLon.HasValue
            };
            schema.ValidateMinimumOptionalFields(DtoName, isPresent, 1);
            return schema;
        }

        public virtual string LookupKey => string.Format("{0} {1} {2} {3} {4}", SurveyId, FormatString(SiteId), FormatDouble(AdHocLat), FormatDouble(AdHocLon), FormatDate(MeasureDateTime));
    }

    public class VegShrubDTO : VegDataDTO, IKeyed
    {
        private const string dtoName = "shrub";
        protected override string DtoName { get{ return dtoName; } }

        public string ShrubSpeciesId { get; set; }
        public string SizeClass { get; set; }
        public uint? Count { get; set; }
        public string Comments { get; set; }

        public VegShrubDTO():base()
        { }

        public VegShrubDTO(Dictionary<string, string> values):base(values)
        {
            ShrubSpeciesId = (string)schema.Parse(values, "ShrubSpeciesId");
            SizeClass = (string)schema.Parse(values, "SizeClass");
            Count = (uint?)schema.Parse(values, "Count");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = BaseSchema(dtoName);
            schema.Add("ShrubSpeciesId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("SizeClass", typeof(string), SchemaEntryType.Normal, 1000, NullableType.IsNotNullable);
            schema.Add("Count", typeof(uint), SchemaEntryType.Normal, 0, NullableType.IsNotNullable);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 1000, NullableType.IsNullable);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = base.Values();
            result.Add("ShrubSpeciesId", FormatString(ShrubSpeciesId));
            result.Add("SizeClass", FormatString(SizeClass));
            result.Add("Count", FormatUnsignedInteger(Count));
            result.Add("Comments", FormatString(Comments));
            return result;
        }
        public override void Validate()
        {
            Schema schema = base.PreValidate();
            schema.ValidateField(ShrubSpeciesId, "ShrubSpeciesId");
            schema.ValidateField(SizeClass, "SizeClass");
            schema.ValidateField(Count, "Count");
            schema.ValidateField(Comments, "Comments");
        }

        public override string LookupKey
        {
            get { return base.LookupKey + " " + this.ShrubSpeciesId; }
        }
    }

    public class VegHerbDTO : VegDataDTO, IKeyed
    {
        private const string dtoName = "herb";
        protected override string DtoName { get { return dtoName; } }

        public string HerbSpeciesId { get; set; }
        public double? PctCover { get; set; }
        public string Comments { get; set; }

        public VegHerbDTO() : base()
        { }

        public VegHerbDTO(Dictionary<string, string> values) : base(values)
        {
            HerbSpeciesId = (string)schema.Parse(values, "HerbSpeciesId");
            PctCover = (double)schema.Parse(values, "PctCover");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = BaseSchema(dtoName);
            schema.Add("HerbSpeciesId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("PctCover", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 1000, NullableType.IsNullable);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = base.Values();
            result.Add("HerbSpeciesId", FormatString(HerbSpeciesId));
            result.Add("PctCover", FormatDouble(PctCover));
            result.Add("Comments", FormatString(Comments));
            return result;
        }
        public override void Validate()
        {
            Schema schema = base.PreValidate();
            schema.ValidateField(HerbSpeciesId, "HerbSpeciesId");
            schema.ValidateField(PctCover, "PctCover");
            schema.ValidateField(Comments, "Comments");
        }

        public override string LookupKey
        {
            get { return base.LookupKey + " " + this.HerbSpeciesId; }
        }
    }

    public class VegTreeDTO : VegDataDTO, IKeyed
    {
        private const string dtoName = "tree";
        protected override string DtoName { get { return dtoName; } }

        public string TreeSpeciesId { get; set; }
        public double? DBH { get; set; }
        public string Comments { get; set; }

        public VegTreeDTO() : base()
        { }

        public VegTreeDTO(Dictionary<string, string> values) : base(values)
        {
            TreeSpeciesId = (string)schema.Parse(values, "TreeSpeciesId");
            DBH = (double)schema.Parse(values, "DBH");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = BaseSchema(dtoName);
            schema.Add("TreeSpeciesId", typeof(string), SchemaEntryType.ForeignLookupKey, 1000, NullableType.IsNotNullable);
            schema.Add("DBH", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNotNullable);
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 1000, NullableType.IsNullable);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = base.Values();
            result.Add("TreeSpeciesId", FormatString(TreeSpeciesId));
            result.Add("DBH", FormatDouble(DBH));
            result.Add("Comments", FormatString(Comments));
            return result;
        }
        public override void Validate()
        {
            Schema schema = base.PreValidate();
            schema.ValidateField(TreeSpeciesId, "TreeSpeciesId");
            schema.ValidateField(DBH, "DBH");
            schema.ValidateField(Comments, "Comments");
        }

        public override string LookupKey
        {
            get { return base.LookupKey + " " + this.TreeSpeciesId; }
        }
    }
}
