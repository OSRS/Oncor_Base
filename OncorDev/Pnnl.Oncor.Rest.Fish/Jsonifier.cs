using Newtonsoft.Json.Linq;
using Osrs.Oncor.WellKnown.Fish;
using System.Collections.Generic;
namespace Pnnl.Oncor.Rest.Fish
{
	internal static class Jsonifier
	{
		public static JObject ToJson(CatchEffort effort)
		{
			if (effort != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(effort.Identity));
				o.Add("sampleeventid", JsonUtils.ToJson(effort.SampleEventId));
				o.Add("siteid", JsonUtils.ToJson(effort.SiteId));
				o.Add("sampledate", effort.SampleDate);
				if (effort.Location != null)
					o.Add("location", effort.Location.ToString());
				if (effort.CatchMethod != null)
					o.Add("catchmethod", effort.CatchMethod);
				if (effort.Strata != null)
					o.Add("strata", effort.Strata);
				o.Add("depth", effort.Depth);
				o.Add("ph", effort.pH);
				o.Add("temp", effort.Temp);
				o.Add("do", effort.DO);
				o.Add("salinity", effort.Salinity);
				o.Add("velocity", effort.Velocity);
				if (effort.Description != null)
					o.Add(JsonUtils.Description, effort.Description);
				o.Add("isprivate", effort.IsPrivate);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<CatchEffort> efforts)
		{
			if (efforts != null)
			{
				JArray o = new JArray();
				foreach (CatchEffort eff in efforts)
				{
					if (eff != null)
						o.Add(ToJson(eff));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(CatchMetric metric)
		{
			if (metric != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(metric.Identity));
				o.Add("catcheffortid", JsonUtils.ToJson(metric.CatchEffortId));
				o.Add("metrictype", metric.MetricType);
				o.Add("value", metric.Value);
				if (metric.Description != null)
					o.Add(JsonUtils.Description, metric.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<CatchMetric> metrics)
		{
			if (metrics != null)
			{
				JArray o = new JArray();
				foreach (CatchMetric metric in metrics)
				{
					if (metric != null)
						o.Add(ToJson(metric));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(FishCount count)
		{
			if (count != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(count.Identity));
				o.Add("catcheffortid", JsonUtils.ToJson(count.CatchEffortId));
				o.Add("taxaid", JsonUtils.ToJson(count.TaxaId));
				o.Add("count", count.Count);
				if (count.Description != null)
					o.Add(JsonUtils.Description, count.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<FishCount> counts)
		{
			if (counts != null)
			{
				JArray o = new JArray();
				foreach (FishCount count in counts)
				{
					if (count != null)
						o.Add(ToJson(count));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(Osrs.Oncor.WellKnown.Fish.Fish fish)
		{
			if (fish != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(fish.Identity));
				o.Add("catcheffortid", JsonUtils.ToJson(fish.CatchEffortId));
				o.Add("taxaid", JsonUtils.ToJson(fish.TaxaId));
				o.Add("lengthstandard", fish.LengthStandard);
				o.Add("lengthfork", fish.LengthFork);
				o.Add("lengthtotal", fish.LengthTotal);
				o.Add("weight", fish.Weight);
				if (fish.AdClipped != null)
					o.Add("adclipped", fish.AdClipped);
				if (fish.CWT != null)
					o.Add("cwt", fish.CWT);
				if (fish.Description != null)
					o.Add(JsonUtils.Description, fish.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<Osrs.Oncor.WellKnown.Fish.Fish> fishes)
		{
			if (fishes != null)
			{
				JArray o = new JArray();
				foreach (Osrs.Oncor.WellKnown.Fish.Fish fish in fishes)
				{
					if (fish != null)
						o.Add(ToJson(fish));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(NetHaulEvent haul)
		{
			if (haul != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(haul.Identity));
				o.Add("catcheffortid", JsonUtils.ToJson(haul.CatchEffortId));
				o.Add("netid", JsonUtils.ToJson(haul.NetId));
				o.Add("areasampled", haul.AreaSampled);
				o.Add("volumesampled", haul.VolumeSampled);
				if (haul.Description != null)
					o.Add(JsonUtils.Description, haul.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<NetHaulEvent> hauls)
		{
			if (hauls != null)
			{
				JArray o = new JArray();
				foreach (NetHaulEvent haul in hauls)
				{
					if (haul != null)
						o.Add(ToJson(haul));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(FishIdTag tag)
		{
			if (tag != null)
			{
				JObject o = new JObject();
				o.Add("fishid", JsonUtils.ToJson(tag.FishId));
				o.Add("tagcode", tag.TagCode);
				o.Add("tagtype", tag.TagType);
				if (tag.TagManufacturer != null)
					o.Add("tagmanufacturer", tag.TagManufacturer);
				if (tag.Description != null)
					o.Add(JsonUtils.Description, tag.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<FishIdTag> tags)
		{
			if (tags != null)
			{
				JArray o = new JArray();
				foreach (FishIdTag tag in tags)
				{
					if (tag != null)
						o.Add(ToJson(tag));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(FishGenetics gene)
		{
			if (gene != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, gene.Identity);
				o.Add("fishid", JsonUtils.ToJson(gene.FishId));
				if (gene.GeneticSampleId != null)
					o.Add("geneticsampleid", gene.GeneticSampleId);
				if (gene.LabSampleId != null)
					o.Add("labsampleid", gene.LabSampleId);
				if (gene.StockEstimates != null)
				{
					JArray estimates = new JArray();
					foreach (StockEstimate est in gene.StockEstimates)
					{
						estimates.Add(ToJson(est));
					}
					o.Add("stockestimates", estimates);
				}
				if (gene.Description != null)
					o.Add(JsonUtils.Description, gene.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<FishGenetics> genes)
		{
			if (genes != null)
			{
				JArray o = new JArray();
				foreach (FishGenetics gene in genes)
				{
					if (gene != null)
						o.Add(ToJson(gene));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(FishDiet diet)
		{
			if (diet != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(diet.Identity));
				o.Add("fishid", JsonUtils.ToJson(diet.FishId));
				o.Add("taxaid", JsonUtils.ToJson(diet.TaxaId));
				if (diet.VialId != null)
					o.Add("vialid", diet.VialId);
				if (diet.GutSampleId != null)
					o.Add("gutsampleid", diet.GutSampleId);
				if (diet.LifeStage != null)
					o.Add("lifestage", diet.LifeStage);
				o.Add("count", diet.Count);
				o.Add("wholeanimalweighed", diet.WholeAnimalsWeighed);
				o.Add("individualmass", diet.IndividualMass);
				o.Add("samplemass", diet.SampleMass);
				if (diet.Description != null)
					o.Add(JsonUtils.Description, diet.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<FishDiet> diets)
		{
			if (diets != null)
			{
				JArray o = new JArray();
				foreach (FishDiet diet in diets)
				{
					if (diet != null)
						o.Add(ToJson(diet));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(StockEstimate estimate)
		{
			if (estimate != null)
			{
				JObject o = new JObject();
				o.Add("stock", estimate.Stock);
				o.Add("prob", estimate.Probability);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<StockEstimate> estimates)
		{
			if (estimates != null)
			{
				JArray o = new JArray();
				foreach (StockEstimate estimate in estimates)
				{
					if (estimate != null)
						o.Add(ToJson(estimate));
				}
				return o;
			}
			return null;
		}
	}
}
