using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs
{
    public class GeneticDTO : BaseDTO, IKeyed
    {
        public string FishId { get; set; }
        public string GeneticSampleId { get; set; }
        public string LabSampleId { get; set; }
        public string BestStockEstimate { get; set; }
        public double? ProbabilityBest { get; set; }
        public string SecondStockEstimate { get; set; }
        public double? ProbabilitySecondBest { get; set; }
        public string ThirdStockEstimate { get; set; }
        public double? ProbabilityThirdBest { get; set; }
        public string Comments { get; set; }

        public GeneticDTO()
        {
        }

        public GeneticDTO(Dictionary<string, string> values)
        {
            Schema schema = GetSchema();
            schema.ValidationIssues = ValidationIssues;
            FishId = (string)schema.Parse(values, "Fish ID");
            GeneticSampleId = (string)schema.Parse(values, "Genetic Sample ID");
            LabSampleId = (string)schema.Parse(values, "Lab Sample ID");
            BestStockEstimate = (string)schema.Parse(values, "Best Stock Estimate");
            ProbabilityBest = (double?)schema.Parse(values, "Probability Best");
            SecondStockEstimate = (string)schema.Parse(values, "Second Stock Estimate");
            ProbabilitySecondBest = (double?)schema.Parse(values, "Probability Second Best");
            ThirdStockEstimate = (string)schema.Parse(values, "Third Stock Estimate");
            ProbabilityThirdBest = (double?)schema.Parse(values, "Probability Third Best");
            Comments = (string)schema.Parse(values, "Comments");
        }

        public override Schema Schema => GetSchema();

        public static Schema GetSchema()
        {
            Schema schema = new Schema(SchemaType.LookupSchema, "genetic");
            schema.Add("Fish ID", typeof(string), SchemaEntryType.ForeignMeasurementKey, 1000, NullableType.IsNotNullable);
            schema.Add("Genetic Sample ID", typeof(string), SchemaEntryType.Normal, 1000, NullableType.IsNotNullable);
            schema.Add("Lab Sample ID", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Best Stock Estimate", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Probability Best", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 100.0));
            schema.Add("Second Stock Estimate", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Probability Second Best", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 100.0));
            schema.Add("Third Stock Estimate", typeof(string), SchemaEntryType.Normal, 1000);
            schema.Add("Probability Third Best", typeof(double), SchemaEntryType.Normal, 0, NullableType.IsNullable, new DoubleRange(0.0, 100.0));
            schema.Add("Comments", typeof(string), SchemaEntryType.Normal, 8000);
            return schema;
        }

        public override Dictionary<string, string> Values()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Fish ID", FishId);
            result.Add("Genetic Sample ID", GeneticSampleId);
            result.Add("Lab Sample ID", LabSampleId);
            result.Add("Best Stock Estimate", BestStockEstimate);
            result.Add("Probability Best", FormatDouble(ProbabilityBest));
            result.Add("Second Stock Estimate", SecondStockEstimate);
            result.Add("Probability Second Best", FormatDouble(ProbabilitySecondBest));
            result.Add("Third Stock Estimate", ThirdStockEstimate);
            result.Add("Probability Third Best", FormatDouble(ProbabilityThirdBest));
            result.Add("Comments", Comments);
            return result;
        }

        public override void Validate()
        {
            Schema schema = Schema;
            schema.ValidationIssues = ValidationIssues;
            schema.ValidateField(FishId, "Fish ID");
            schema.ValidateField(GeneticSampleId, "Genetic Sample ID");
            schema.ValidateField(LabSampleId, "Lab Sample ID");
            schema.ValidateField(BestStockEstimate, "Best Stock Estimate");
            schema.ValidateField(ProbabilityBest, "Probability Best");
            schema.ValidateField(SecondStockEstimate, "Second Stock Estimate");
            schema.ValidateField(ProbabilitySecondBest, "Probability Second Best");
            schema.ValidateField(ThirdStockEstimate, "Third Stock Estimate");
            schema.ValidateField(ProbabilityThirdBest, "Probability Third Best");
            schema.ValidateField(Comments, "Comments");
        }

        public string LookupKey { get { return FishId + " " + GeneticSampleId; } }
    }
}
