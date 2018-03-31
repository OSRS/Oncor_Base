using Newtonsoft.Json.Linq;
using Osrs.Oncor.WellKnown.Vegetation;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Vegetation
{
	internal static class Jsonifier
	{

		public static JObject ToJson(VegSurvey survey)
		{
			if (survey != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(survey.Identity));
				o.Add("sampleeventid", JsonUtils.ToJson(survey.SampleEventId));
				o.Add("siteid", JsonUtils.ToJson(survey.SiteId));
				o.Add("plottypeid", JsonUtils.ToJson(survey.PlotTypeId));
				if (survey.Location != null)
					o.Add("location", survey.Location.ToString());
				o.Add("area", survey.Area);
				if (survey.ElevationRange != null)
				{
					o.Add("elevationmin", survey.ElevationRange.Min);
					o.Add("elevationmax", survey.ElevationRange.Max);
				}
				if (survey.Description != null)
					o.Add(JsonUtils.Description, survey.Description);
				o.Add("isprivate", survey.IsPrivate);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<VegSurvey> surveys)
		{
			if (surveys != null)
			{
				JArray o = new JArray();
				foreach (VegSurvey survey in surveys)
				{
					if (survey != null)
						o.Add(ToJson(survey));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(VegSample samp)
		{
			if (samp != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(samp.Identity));
				o.Add("vegsurveyid", JsonUtils.ToJson(samp.VegSurveyId));
				o.Add("siteid", JsonUtils.ToJson(samp.SiteId));
				if (samp.When != null)
					o.Add("when", samp.When);
				if (samp.Location != null)
					o.Add("location", samp.Location.ToString());
				if (samp.ElevationRange != null)
				{
					o.Add("elevationmin", samp.ElevationRange.Min);
					o.Add("elevationmax", samp.ElevationRange.Max);
				}
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<VegSample> vegs)
		{
			if (vegs != null)
			{
				JArray o = new JArray();
				foreach (VegSample samp in vegs)
				{
					if (samp != null)
						o.Add(ToJson(samp));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(HerbSample herb)
		{
			if (herb != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(herb.Identity));
				o.Add("vegsampleid", JsonUtils.ToJson(herb.VegSampleId));
				o.Add("taxaunitid", JsonUtils.ToJson(herb.TaxaUnitId));
				o.Add("percentcover", herb.PercentCover);
				if (herb.Description != null)
					o.Add(JsonUtils.Description, herb.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<HerbSample> herbs)
		{
			if (herbs != null)
			{
				JArray o = new JArray();
				foreach (HerbSample herb in herbs)
				{
					if (herb != null)
						o.Add(ToJson(herb));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(ShrubSample shrub)
		{
			if (shrub != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(shrub.Identity));
				o.Add("vegsampleid", JsonUtils.ToJson(shrub.VegSampleId));
				o.Add("taxaunitid", JsonUtils.ToJson(shrub.TaxaUnitId));
				if (shrub.SizeClass != null)
					o.Add("sizeclass", shrub.SizeClass);
				o.Add("count", shrub.Count);
				if (shrub.Description != null)
					o.Add(JsonUtils.Description, shrub.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<ShrubSample> shrubs)
		{
			if (shrubs != null)
			{
				JArray o = new JArray();
				foreach (ShrubSample shrub in shrubs)
				{
					if (shrub != null)
						o.Add(ToJson(shrub));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(TreeSample tree)
		{
			if (tree != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(tree.Identity));
				o.Add("vegsampleid", JsonUtils.ToJson(tree.VegSampleId));
				o.Add("taxaunitid", JsonUtils.ToJson(tree.TaxaUnitId));
				o.Add("diameterbreasthigh", tree.DiameterBreastHigh);
				if (tree.Description != null)
					o.Add(JsonUtils.Description, tree.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<TreeSample> trees)
		{
			if (trees != null)
			{
				JArray o = new JArray();
				foreach (TreeSample tree in trees)
				{
					if (tree != null)
						o.Add(ToJson(tree));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(VegPlotType plot)
		{
			if (plot != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(plot.Identity));
				o.Add(JsonUtils.Name, plot.Name);
				if (plot.Description != null)
					o.Add(JsonUtils.Description, plot.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<VegPlotType> plots)
		{
			if (plots != null)
			{
				JArray o = new JArray();
				foreach (VegPlotType plot in plots)
				{
					if (plot != null)
						o.Add(ToJson(plot));
				}
				return o;
			}
			return null;
		}

	}
}
