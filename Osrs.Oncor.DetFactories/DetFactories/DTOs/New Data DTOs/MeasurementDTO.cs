using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class MeasurementDTO : BaseDTO, IKeyed
    {
        private const string dtoName = "measurement";
        public string DeployCode { get; set; }
        public DateTime? MeasureDateTime { get; set; }
        public double? Temperature { get; set; }
        public double? SurfaceElevation { get; set; }
        public double? pH { get; set; }
        public double? DO { get; set; }
        public double? Conductivity { get; set; }
        public double? Salinity { get; set; }
        public double? Velocity { get; set; }

        public MeasurementDTO()
        {
        }

        public MeasurementDTO(Dictionary<string, string> values)
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            DeployCode = (string)schema.Parse(values, "DeployCode");
            MeasureDateTime = (DateTime?)schema.Parse(values, "MeasureDateTime");
            Temperature = (double?)schema.Parse(values, "Temperature");
            SurfaceElevation = (double?)schema.Parse(values, "SurfaceElevation");
            pH = (double?)schema.Parse(values, "pH");
            DO = (double?)schema.Parse(values, "DO");
            Conductivity = (double?)schema.Parse(values, "Conductivity");
            Salinity = (double?)schema.Parse(values, "Salinity");
            Velocity = (double?)schema.Parse(values, "Velocity");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.MeasurementSchema, "measurement");
            schema.Add("DeployCode", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("MeasureDateTime", typeof(DateTime), SchemaEntryType.Normal, 0, NullableType.IsNotNullable);
            schema.Add("Temperature", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(-273.15, 100.0));
            schema.Add("SurfaceElevation", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(-50000.0, 800000.0));
            schema.Add("pH", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(1.0, 14.0));
            schema.Add("DO", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 30.0));
            schema.Add("Conductivity", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 1E6));
            schema.Add("Salinity", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 40.0));
            schema.Add("Velocity", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 6.0));
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("DeployCode", DeployCode);
            result.Add("MeasureDateTime", FormatDate(MeasureDateTime));
            result.Add("Temperature", FormatDouble(Temperature));
            result.Add("SurfaceElevation", FormatDouble(SurfaceElevation));
            result.Add("pH", FormatDouble(pH));
            result.Add("DO", FormatDouble(DO));
            result.Add("Conductivity", FormatDouble(Conductivity));
            result.Add("Salinity", FormatDouble(Salinity));
            result.Add("Velocity", FormatDouble(Velocity));
            return result;
        }
        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(DeployCode, "DeployCode");
            schema.ValidateField(MeasureDateTime, "MeasureDateTime");
            schema.ValidateField(Temperature, "Temperature");
            schema.ValidateField(SurfaceElevation, "SurfaceElevation");
            schema.ValidateField(pH, "pH");
            schema.ValidateField(DO, "DO");
            schema.ValidateField(Conductivity, "Conductivity");
            schema.ValidateField(Salinity, "Salinity");
            schema.ValidateField(Velocity, "Velocity");
            bool[] isPresent = new[]
            {
                Temperature.HasValue, SurfaceElevation.HasValue,
                pH.HasValue, DO.HasValue, Conductivity.HasValue,
                Salinity.HasValue, Velocity.HasValue
            };
            schema.ValidateMinimumOptionalFields(dtoName, isPresent, 1);
        }

        public string LookupKey => string.Format("{0} {1}", DeployCode, FormatDate(MeasureDateTime));
    }
}
