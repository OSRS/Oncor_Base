using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Numerics.Spatial.Geometry;
using Osrs.Oncor.EntityBundles;
using Osrs.Oncor.Excel;
using Osrs.Oncor.FileStore;
using Osrs.Oncor.SimpleDb;
using Osrs.Oncor.WellKnown.Fish;
using Osrs.Oncor.WellKnown.Fish.Module;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using Osrs.WellKnown.FieldActivities;
using Osrs.WellKnown.Organizations;
using Osrs.WellKnown.Projects;
using Osrs.WellKnown.SensorsAndInstruments;
using Osrs.WellKnown.Sites;
using Osrs.WellKnown.Taxonomy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pnnl.Oncor.Rest.Fish
{
	public sealed class FishHandler : HttpHandlerBase, IServiceHandler
	{
		private const int excelDataCutoff = 5000;
        private string fileExtension = null;
        const string na = "Unknown or Not Authorized";

        private SessionProviderBase sessionProvider;
		private SessionProviderBase SessionProvider
		{
			get
			{
				if (sessionProvider == null)
					sessionProvider = SessionManager.Instance.GetProvider();
				return sessionProvider;
			}
		}

		public string BaseUrl
		{
			get
			{
				return "fish";
			}
		}

		public override void Handle(HttpContext context, CancellationToken cancel)
		{
			if (context != null)
			{
				UserIdentityBase user = Security.Session.GetUser(context);
				if (user != null)
				{
					UserSecurityContext ctx = new UserSecurityContext(user);
					string localUrl = RestUtils.LocalUrl(this, context.Request);
					string meth = RestUtils.StripLocal(this.BaseUrl, localUrl);
					meth = meth.Substring(1);

					if (!string.IsNullOrEmpty(meth))
					{
						if (context.Request.Method == "POST")
						{
							if (meth.Equals("catch", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									HashSet<CompoundIdentity> ids = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["ids"] != null)
											ids = JsonUtils.ToIds(token["ids"]);
									}

									IEnumerable<CatchEffort> efforts = GetCatchEfforts(ctx, ids);
									JArray jefforts = Jsonifier.ToJson(efforts);
									if (jefforts != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jefforts.ToString());
										return;
									}
									else
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
										return;
									}
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
							else if (meth.Equals("haul", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									CompoundIdentity id = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["catchid"] != null)
											id = JsonUtils.ToId(token["catchid"]);
									}

									IEnumerable<NetHaulEvent> hauls = GetNetHaulEvents(ctx, id);
									JArray jhauls = Jsonifier.ToJson(hauls);
									if (jhauls != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jhauls.ToString());
										return;
									}
									else
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
										return;
									}
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
							else if (meth.Equals("count", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									CompoundIdentity id = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["catchid"] != null)
											id = JsonUtils.ToId(token["catchid"]);
									}

									IEnumerable<FishCount> counts = GetFishCounts(ctx, id);
									JArray jcounts = Jsonifier.ToJson(counts);
									if (jcounts != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jcounts.ToString());
										return;
									}
									else
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
										return;
									}
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
							else if (meth.Equals("metric", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									CompoundIdentity id = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["catchid"] != null)
											id = JsonUtils.ToId(token["catchid"]);
									}

									IEnumerable<CatchMetric> metrics = GetCatchMetrics(ctx, id);
									JArray jmetrics = Jsonifier.ToJson(metrics);
									if (jmetrics != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jmetrics.ToString());
										return;
									}
									else
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
										return;
									}
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
							else if (meth.Equals("fish", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									CompoundIdentity id = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["catchid"] != null)
											id = JsonUtils.ToId(token["catchid"]);
									}

									IEnumerable<Osrs.Oncor.WellKnown.Fish.Fish> fishes = GetFishes(ctx, id);
									JArray jfishes = Jsonifier.ToJson(fishes);
									if (jfishes != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jfishes.ToString());
										return;
									}
									else
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
										return;
									}
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
							else if (meth.Equals("fishtag", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									Guid? id = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["fishid"] != null)
											id = Guid.Parse(token["fishid"].ToString());
									}

									IEnumerable<FishIdTag> tags = GetFishIdTags(ctx, id);
									JArray jtags = Jsonifier.ToJson(tags);
									if (jtags != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jtags.ToString());
										return;
									}
									else
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
										return;
									}
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
							else if (meth.Equals("fishdiet", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									Guid? id = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["fishid"] != null)
											id = Guid.Parse(token["fishid"].ToString());
									}

									IEnumerable<FishDiet> diets = GetFishDiets(ctx, id);
									JArray jdiets = Jsonifier.ToJson(diets);
									if (jdiets != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jdiets.ToString());
										return;
									}
									else
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
										return;
									}
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
							else if (meth.Equals("fishdna", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									Guid? id = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["fishid"] != null)
											id = Guid.Parse(token["fishid"].ToString());
									}

									IEnumerable<FishGenetics> genes = GetFishGenetics(ctx, id);
									JArray jgenes = Jsonifier.ToJson(genes);
									if (jgenes != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jgenes.ToString());
										return;
									}
									else
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
										return;
									}
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
							else if (meth.Equals("export", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									ISampleEventProvider sampProvider = FieldActivityManager.Instance.GetSampleEventProvider(ctx);
									IOrganizationProvider orgProvider = OrganizationManager.Instance.GetOrganizationProvider(ctx);
									IFieldTripProvider fieldTripProvider = FieldActivityManager.Instance.GetFieldTripProvider(ctx);
									IFieldActivityProvider fieldActivityProvider = FieldActivityManager.Instance.GetFieldActivityProvider(ctx);
									IProjectProvider projectProvider = ProjectManager.Instance.GetProvider(ctx);

									if (sampProvider != null && orgProvider != null && fieldTripProvider != null &&
										fieldActivityProvider != null && projectProvider != null)
									{

										HashSet<CompoundIdentity> eventIds = null;
										HashSet<CompoundIdentity> effortIds = null;
										HashSet<CompoundIdentity> siteIds = null;
										HashSet<Guid> nativeBundleIds = null;
										DateTime? start = null;
										DateTime? end = null;

										JToken token = JsonUtils.GetDataPayload(context.Request);
										if (token != null)
										{
											if (token["events"] != null)
												eventIds = JsonUtils.ToIds(token["events"]);

											if (token["catchefforts"] != null)
												effortIds = JsonUtils.ToIds(token["catchefforts"]);

											if (token["sites"] != null)
												siteIds = JsonUtils.ToIds(token["sites"]);

											if (token["start"] != null)
												start = JsonUtils.ToDate(token["start"]);

											if (token["end"] != null)
												end = JsonUtils.ToDate(token["end"]);

											if (token["nativeBundleId"] != null)
												nativeBundleIds = JsonUtils.ToGuids(token["nativeBundleId"]);
										}

										IEnumerable<CatchEffort> efforts = GetCatchEfforts(ctx, effortIds);
										if (eventIds != null)
											efforts = efforts.Where(x => eventIds.Contains(x.SampleEventId));
										if (siteIds != null)
											efforts = efforts.Where(x => siteIds.Contains(x.SiteId));
										if (start != null)
											efforts = efforts.Where(x => x.SampleDate >= start);
										if (end != null)
											efforts = efforts.Where(x => x.SampleDate <= end);

										IEnumerable<CatchEffort> filteredEfforts = efforts.ToList();

										//Get sampling events and sites
										List<CompoundIdentity> selectedEvents = filteredEfforts.Select(x => x.SampleEventId).ToList();
										List<CompoundIdentity> selectedSites = filteredEfforts.Select(x => x.SiteId).ToList();
										IEnumerable<Site> sitesData = GetSites(ctx, selectedSites);
										IEnumerable<SamplingEvent> eventsData = sampProvider.Get(selectedEvents);

										//Get orgs and field trips
										List<CompoundIdentity> selected_orgIds = eventsData.Select(x => x.PrincipalOrgId).ToList();
										List<CompoundIdentity> selected_fieldTripIds = eventsData.Select(x => x.FieldTripId).ToList();
										IEnumerable<Organization> orgData = orgProvider.Get(selected_orgIds);
										IEnumerable<FieldTrip> fieldTripData = fieldTripProvider.Get(selected_fieldTripIds);

										//Get field activities
										List<CompoundIdentity> selected_fieldActivityIds = fieldTripData.Select(x => x.FieldActivityId).ToList();
										IEnumerable<FieldActivity> fieldActivityData = fieldActivityProvider.Get(selected_fieldActivityIds);

										//Get projects
										List<CompoundIdentity> selected_projectIds = fieldActivityData.Select(x => x.ProjectId).ToList();
										IEnumerable<Project> projectData = projectProvider.Get(selected_projectIds);


										List<NetHaulEvent> hauls = new List<NetHaulEvent>();
										List<FishCount> counts = new List<FishCount>();
										List<CatchMetric> metrics = new List<CatchMetric>();
										List<Osrs.Oncor.WellKnown.Fish.Fish> fishes = new List<Osrs.Oncor.WellKnown.Fish.Fish>();

										List<FishIdTag> ids = new List<FishIdTag>();
										List<FishDiet> diets = new List<FishDiet>();
										List<FishGenetics> genetics = new List<FishGenetics>();

										foreach (CatchEffort effort in filteredEfforts)
										{
											IEnumerable<NetHaulEvent> haul = GetNetHaulEvents(ctx, effort.Identity);
											if (haul != null)
												hauls.AddRange(haul);
											IEnumerable<FishCount> count = GetFishCounts(ctx, effort.Identity);
											if (count != null)
												counts.AddRange(count);
											IEnumerable<CatchMetric> metric = GetCatchMetrics(ctx, effort.Identity);
											if (metric != null)
												metrics.AddRange(metric);
											IEnumerable<Osrs.Oncor.WellKnown.Fish.Fish> fish = GetFishes(ctx, effort.Identity);
											if (fish != null)
												fishes.AddRange(fish);
										}
                                        
                                        IEnumerable<FishIdTag> id = null;
                                        IEnumerable<FishDiet> diet = null;
                                        IEnumerable<FishGenetics> gene = null;
                                        IFishProvider fishProvider = FishManager.Instance.GetFishProvider(ctx);
                                        List<Guid> fishIds = fishes.Select(fish => fish.Identity).ToList();

                                        if (fishProvider != null && fishIds != null && fishIds.Any())
                                        {
                                            id = fishProvider.GetFishIdTag(fishIds);
                                            diet = fishProvider.GetFishDiet(fishIds);
                                            gene = fishProvider.GetFishGenetics(fishIds);
                                        }
                                        if (id != null)
                                            ids.AddRange(id);
                                        if (diet != null)
                                            diets.AddRange(diet);
                                        if (gene != null)
                                            genetics.AddRange(gene);

                                        Guid fileId = CreateFishFile(orgData, fieldTripData, fieldActivityData, projectData, eventsData, filteredEfforts, hauls,
											counts, metrics, fishes, ids, diets, genetics, sitesData, nativeBundleIds, ctx);

										if (fileId != null)
										{
											JObject o = new JObject();
											o.Add("fileid", fileId.ToString());
                                            o.Add("fileext", fileExtension);
                                            RestUtils.Push(context.Response, JsonOpStatus.Ok, o);
										}
										else
										{
											RestUtils.Push(context.Response, JsonOpStatus.Failed);
										}
										return;
									}
									RestUtils.Push(context.Response, JsonOpStatus.Failed);
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
						}
					}
				}
			}
			context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
		}

		private IEnumerable<CatchEffort> GetCatchEfforts(UserSecurityContext ctx, IEnumerable<CompoundIdentity> ids)
		{
			ICatchEffortProvider effortProvider = FishManager.Instance.GetCatchEffortProvider(ctx);
			IEnumerable<CatchEffort> efforts = null;
			if (effortProvider != null)
			{
				if (ids != null)
					efforts = effortProvider.Get(ids);
				else
					efforts = effortProvider.Get();
			}
			return efforts;
		}

		private IEnumerable<NetHaulEvent> GetNetHaulEvents(UserSecurityContext ctx, CompoundIdentity catchid)
		{
			ICatchHaulProvider haulProvider = FishManager.Instance.GetCatchHaulProvider(ctx);
			IEnumerable<NetHaulEvent> hauls = null;
			if (haulProvider != null)
			{
				if (catchid != null)
					hauls = haulProvider.GetHauls(catchid);
				else
					hauls = haulProvider.GetHauls();
			}
			return hauls;
		}

		private IEnumerable<FishCount> GetFishCounts(UserSecurityContext ctx, CompoundIdentity catchid)
		{
			ICatchHaulProvider haulProvider = FishManager.Instance.GetCatchHaulProvider(ctx);
			IEnumerable<FishCount> counts = null;
			if (haulProvider != null)
			{
				if (catchid != null)
					counts = haulProvider.GetFishCounts(catchid);
				else
					counts = haulProvider.GetFishCounts();
			}
			return counts;
		}

		private IEnumerable<CatchMetric> GetCatchMetrics(UserSecurityContext ctx, CompoundIdentity catchid)
		{
			ICatchHaulProvider haulProvider = FishManager.Instance.GetCatchHaulProvider(ctx);
			IEnumerable<CatchMetric> metrics = null;
			if (haulProvider != null)
			{
				if (catchid != null)
					metrics = haulProvider.GetMetrics(catchid);
				else
					metrics = haulProvider.GetMetrics();
			}
			return metrics;
		}

		private IEnumerable<Osrs.Oncor.WellKnown.Fish.Fish> GetFishes(UserSecurityContext ctx, CompoundIdentity catchid)
		{
			IFishProvider fishProvider = FishManager.Instance.GetFishProvider(ctx);
			IEnumerable<Osrs.Oncor.WellKnown.Fish.Fish> fishes = null;
			if (fishProvider != null)
			{
				if (catchid != null)
					fishes = fishProvider.GetFish(catchid);
				else
					fishes = fishProvider.GetFish();
			}
			return fishes;
		}

		private IEnumerable<FishIdTag> GetFishIdTags(UserSecurityContext ctx, Guid? fishid)
		{
			IFishProvider fishProvider = FishManager.Instance.GetFishProvider(ctx);
			IEnumerable<FishIdTag> tags = null;
			if (fishProvider != null)
			{
				if (fishid.HasValue)
					tags = fishProvider.GetFishIdTag(fishid.Value);
				else
					tags = fishProvider.GetFishIdTag();
			}
			return tags;
		}

		private IEnumerable<FishDiet> GetFishDiets(UserSecurityContext ctx, Guid? fishid)
		{
			IFishProvider fishProvider = FishManager.Instance.GetFishProvider(ctx);
			IEnumerable<FishDiet> diets = null;
			if (fishProvider != null)
			{
				if (fishid.HasValue)
					diets = fishProvider.GetFishDiet(fishid.Value);
				else
					diets = fishProvider.GetFishDiet();
			}
			return diets;
		}

		private IEnumerable<FishGenetics> GetFishGenetics(UserSecurityContext ctx, Guid? fishid)
		{
			IFishProvider fishProvider = FishManager.Instance.GetFishProvider(ctx);
			IEnumerable<FishGenetics> genes = null;
			if (fishProvider != null)
			{
				if (fishid.HasValue)
					genes = fishProvider.GetFishGenetics(fishid.Value);
				else
					genes = fishProvider.GetFishGenetics();
			}
			return genes;
		}

		private List<Site> GetSites(UserSecurityContext ctx, IEnumerable<CompoundIdentity> siteIds)
		{
			ISiteProvider provider = SiteManager.Instance.GetSiteProvider(ctx);
			List<Site> sites = new List<Site>();
			Site s = null;
			foreach (CompoundIdentity id in siteIds)
			{
				s = provider.Get(id);
				if (s != null)
					sites.Add(s);
			}
			return sites;
		}

		private Guid CreateFishFile(IEnumerable<Organization> orgs, IEnumerable<FieldTrip> trips, IEnumerable<FieldActivity> activities,
			IEnumerable<Project> projects, IEnumerable<SamplingEvent> events, IEnumerable<CatchEffort> efforts, IEnumerable<NetHaulEvent> hauls,
			IEnumerable<FishCount> counts, IEnumerable<CatchMetric> metrics, IEnumerable<Osrs.Oncor.WellKnown.Fish.Fish> fishes,
			IEnumerable<FishIdTag> tags, IEnumerable<FishDiet> diets, IEnumerable<FishGenetics> genetics, IEnumerable<Site> sites,
			HashSet<Guid> nativeBundleIds, UserSecurityContext ctx)
		{
			int i = 1;
			Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict = new Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>>();
			foreach (SamplingEvent samp in events)
			{
				if (!eventDict.ContainsKey(samp.Identity))
				{
					eventDict.Add(samp.Identity, new Tuple<int, SamplingEvent>(i, samp));
					i++;
				}
			}

			i = 1;
			Dictionary<CompoundIdentity, Tuple<int, CatchEffort>> effortDict = new Dictionary<CompoundIdentity, Tuple<int, CatchEffort>>();
			foreach (CatchEffort eff in efforts)
			{
				if (!effortDict.ContainsKey(eff.Identity))
				{
					effortDict.Add(eff.Identity, new Tuple<int, CatchEffort>(i, eff));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, NetHaulEvent>> haulDict = new Dictionary<Guid, Tuple<int, NetHaulEvent>>();
			foreach (NetHaulEvent haul in hauls)
			{
				if (!haulDict.ContainsKey(haul.Identity))
				{
					haulDict.Add(haul.Identity, new Tuple<int, NetHaulEvent>(i, haul));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, FishCount>> countDict = new Dictionary<Guid, Tuple<int, FishCount>>();
			foreach (FishCount count in counts)
			{
				if (!countDict.ContainsKey(count.Identity))
				{
					countDict.Add(count.Identity, new Tuple<int, FishCount>(i, count));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, CatchMetric>> metricDict = new Dictionary<Guid, Tuple<int, CatchMetric>>();
			foreach (CatchMetric met in metrics)
			{
				if (!metricDict.ContainsKey(met.Identity))
				{
					metricDict.Add(met.Identity, new Tuple<int, CatchMetric>(i, met));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, Osrs.Oncor.WellKnown.Fish.Fish>> fishDict = new Dictionary<Guid, Tuple<int, Osrs.Oncor.WellKnown.Fish.Fish>>();
			foreach (var fish in fishes)
			{
				if (!fishDict.ContainsKey(fish.Identity))
				{
					fishDict.Add(fish.Identity, new Tuple<int, Osrs.Oncor.WellKnown.Fish.Fish>(i, fish));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, FishIdTag>> tagDict = new Dictionary<Guid, Tuple<int, FishIdTag>>();
			foreach (FishIdTag tag in tags)
			{
				if (!tagDict.ContainsKey(tag.Identity))
				{
					tagDict.Add(tag.Identity, new Tuple<int, FishIdTag>(i, tag));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, FishDiet>> dietDict = new Dictionary<Guid, Tuple<int, FishDiet>>();
			foreach (FishDiet diet in diets)
			{
				if (!dietDict.ContainsKey(diet.Identity))
				{
					dietDict.Add(diet.Identity, new Tuple<int, FishDiet>(i, diet));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, FishGenetics>> geneDict = new Dictionary<Guid, Tuple<int, FishGenetics>>();
			foreach (FishGenetics gene in genetics)
			{
				if (!geneDict.ContainsKey(gene.Identity))
				{
					geneDict.Add(gene.Identity, new Tuple<int, FishGenetics>(i, gene));
					i++;
				}
			}

			i = 1;
			Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict = new Dictionary<CompoundIdentity, Tuple<int, Site>>();
			foreach (Site site in sites)
			{
				if (!siteDict.ContainsKey(site.Identity))
				{
					siteDict.Add(site.Identity, new Tuple<int, Site>(i, site));
					i++;
				}
			}

			Dictionary<CompoundIdentity, Organization> orgDict = new Dictionary<CompoundIdentity, Organization>();
			foreach (Organization org in orgs)
			{
				if (!orgDict.ContainsKey(org.Identity))
				{
					orgDict.Add(org.Identity, org);
				}
			}

			Dictionary<CompoundIdentity, FieldTrip> fieldTripDict = new Dictionary<CompoundIdentity, FieldTrip>();
			foreach (FieldTrip ft in trips)
			{
				if (!fieldTripDict.ContainsKey(ft.Identity))
				{
					fieldTripDict.Add(ft.Identity, ft);
				}
			}

			Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict = new Dictionary<CompoundIdentity, FieldActivity>();
			foreach (FieldActivity fa in activities)
			{
				if (!fieldActivityDict.ContainsKey(fa.Identity))
				{
					fieldActivityDict.Add(fa.Identity, fa);
				}
			}

			Dictionary<CompoundIdentity, Project> projectDict = new Dictionary<CompoundIdentity, Project>();
			foreach (Project proj in projects)
			{
				if (!projectDict.ContainsKey(proj.Identity))
				{
					projectDict.Add(proj.Identity, proj);
				}
			}

			List<CompoundIdentity> taxaIds = new List<CompoundIdentity>();
			taxaIds.AddRange(countDict.Select(x => x.Value.Item2.TaxaId));
			taxaIds.AddRange(fishDict.Select(x => x.Value.Item2.TaxaId));
			taxaIds.AddRange(dietDict.Select(x => x.Value.Item2.TaxaId));
			ITaxaUnitProvider taxaProv = TaxonomyManager.Instance.GetTaxaUnitProvider(ctx);
			Dictionary<CompoundIdentity, TaxaUnit> taxaDict = new Dictionary<CompoundIdentity, TaxaUnit>();
			if (taxaProv != null)
			{
				var taxaUnits = taxaProv.Get(taxaIds);
				foreach (TaxaUnit tx in taxaUnits)
				{
					if (!taxaDict.ContainsKey(tx.Identity))
					{
						taxaDict.Add(tx.Identity, tx);
					}
				}
			}
			
			List<CompoundIdentity> netIds = new List<CompoundIdentity>();
			netIds.AddRange(haulDict.Select(x => x.Value.Item2.NetId));
			IInstrumentProvider instrumentProv = InstrumentManager.Instance.GetInstrumentProvider(ctx);
			Dictionary<CompoundIdentity, Instrument> netDict = new Dictionary<CompoundIdentity, Instrument>();
			if (instrumentProv != null)
			{
				var nets = instrumentProv.Get(netIds);
				foreach (Instrument net in nets)
				{
					if (!netDict.ContainsKey(net.Identity))
					{
						netDict.Add(net.Identity, net);
					}
				}
			}

			EntityBundleProvider entProv = EntityBundleManager.Instance.GetProvider(ctx);
			HashSet<CompoundIdentity> nativeIds = new HashSet<CompoundIdentity>();
			if (nativeBundleIds != null && entProv != null)
			{
				IEnumerable<EntityBundle> nativeBundles = entProv.Get(nativeBundleIds);
				foreach (EntityBundle ent in nativeBundles)
				{
					foreach (BundleElement entry in ent.Elements)
					{
						nativeIds.Add(entry.EntityId);
					}
				}
			}

			int entryCount = eventDict.Count + effortDict.Count + haulDict.Count + countDict.Count + metricDict.Count + fishDict.Count +
				tagDict.Count + dietDict.Count + geneDict.Count + siteDict.Count + orgDict.Count + fieldTripDict.Count + fieldActivityDict.Count +
				projectDict.Count;

			if (entryCount <= excelDataCutoff)
			{
                fileExtension = "xlsx";
                return CreateExcelFile(eventDict, effortDict, haulDict, countDict, metricDict, fishDict, tagDict, dietDict, geneDict,
					siteDict, orgDict, fieldTripDict, fieldActivityDict, projectDict, taxaDict, netDict, nativeIds, ctx);
			}
			else
			{
                fileExtension = "zip";
                return CreateCsvFile(eventDict, effortDict, haulDict, countDict, metricDict, fishDict, tagDict, dietDict, geneDict,
					siteDict, orgDict, fieldTripDict, fieldActivityDict, projectDict, taxaDict, netDict, nativeIds, ctx);
			}
		}

		private Guid CreateCsvFile(Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict, Dictionary<CompoundIdentity, Tuple<int, CatchEffort>> effortDict,
			Dictionary<Guid, Tuple<int, NetHaulEvent>> haulDict, Dictionary<Guid, Tuple<int, FishCount>> countDict, Dictionary<Guid, Tuple<int, CatchMetric>> metricDict,
			Dictionary<Guid, Tuple<int, Osrs.Oncor.WellKnown.Fish.Fish>> fishDict, Dictionary<Guid, Tuple<int, FishIdTag>> tagDict, Dictionary<Guid, Tuple<int, FishDiet>> dietDict,
			Dictionary<Guid, Tuple<int, FishGenetics>> geneDict, Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict, Dictionary<CompoundIdentity, Organization> orgDict,
			Dictionary<CompoundIdentity, FieldTrip> fieldTripDict, Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict, Dictionary<CompoundIdentity, Project> projectDict,
			Dictionary<CompoundIdentity, TaxaUnit> taxaDict, Dictionary<CompoundIdentity, Instrument> netDict, HashSet<CompoundIdentity> nativeIds, UserSecurityContext ctx)
		{
			IFileStoreProvider provider = FileStoreManager.Instance.GetProvider();

			//Setting up temp file
			FilestoreFile fishFile = provider.MakeTemp(DateTime.UtcNow.AddHours(4));
			CsvDb csv = CsvDb.Create(fishFile);

			CreateUpperCsv(csv, eventDict, orgDict, fieldTripDict, fieldActivityDict, projectDict);
			CreateEffortCsv(csv, effortDict, eventDict, siteDict);
			CreateHaulsCsv(csv, haulDict, effortDict, netDict, ctx);
			CreateCountsCsv(csv, taxaDict, effortDict, countDict, nativeIds);
			CreateMetricCsv(csv, metricDict, effortDict);
			CreateFishCsv(csv, fishDict, taxaDict, effortDict, nativeIds);
			CreateIdTagCsv(csv, tagDict, fishDict);
			CreateDietCsv(csv, fishDict, dietDict, taxaDict, nativeIds);
			CreateGeneticsCsv(csv, geneDict, fishDict);
			CreateSitesCsv(csv, siteDict);

			csv.Flush();
			csv.Dispose();
			fishFile.Close();
			return fishFile.FileId;
		}

		private static void CreateUpperCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict,
			Dictionary<CompoundIdentity, Organization> orgDict, Dictionary<CompoundIdentity, FieldTrip> fieldTripDict,
			Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict, Dictionary<CompoundIdentity, Project> projectDict)
		{
            string[] cols = new string[] { "Org", "FieldTrip", "Activity", "Project", "SampleEvent", "SampleEventKey", "SampleEventDesc" };

            ITable upper = db.Create("SamplingEvents", cols);

			const string na = "Unknown or Not Authorized";
			var orderedEvents = eventDict.OrderBy(x => x.Value.Item1);
			foreach (var evt in orderedEvents)
			{
				string orgName = na;
				if (orgDict.ContainsKey(evt.Value.Item2.PrincipalOrgId))
					orgName = orgDict[evt.Value.Item2.PrincipalOrgId].Name;
				string ftripName = na;
				string factivityName = na;
				string projName = na;
				if (fieldTripDict.ContainsKey(evt.Value.Item2.FieldTripId))
				{
					FieldTrip ftrip = fieldTripDict[evt.Value.Item2.FieldTripId];
					ftripName = ftrip.Name;
					if (fieldActivityDict.ContainsKey(ftrip.FieldActivityId))
					{
						FieldActivity factivity = fieldActivityDict[ftrip.FieldActivityId];
						factivityName = factivity.Name;
						if (projectDict.ContainsKey(factivity.ProjectId))
							projName = projectDict[factivity.ProjectId].Name;
					}
				}

				IRow r = upper.CreateRow();
				r[0] = orgName;
				r[1] = ftripName;
				r[2] = factivityName;
				r[3] = projName;
				r[4] = evt.Value.Item2.Name;
				r[5] = evt.Value.Item1.ToString();
				r[6] = evt.Value.Item2.Description;
				upper.AddRow(r);
			}
			upper.Flush();
			return;
		}

		private static void CreateEffortCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, CatchEffort>> effortDict,
			Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict, Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict)
		{
			string[] cols = new string[] { "CatchEffortKey", "SamplingEventKey", "SiteKey", "Time", "AdHocLocation",
			"Method", "Strata", "Depth", "pH", "Temp", "DO", "Salinity", "Velocity", "Description", "IsPrivate" };

			ITable efforts = db.Create("CatchEfforts", cols);

			var orderedEfforts = effortDict.OrderBy(x => x.Value.Item1);
			foreach (var eff in orderedEfforts)
			{
				CatchEffort effort = eff.Value.Item2;
				IRow r = efforts.CreateRow();
				r[0] = eff.Value.Item1.ToString();
				r[1] = eventDict[effort.SampleEventId].Item1.ToString();

                //effort.SiteId could be a dangling reference
                string siteFK = na;
                if (siteDict.ContainsKey(effort.SiteId))
                {
                    siteFK = siteDict[effort.SiteId].Item1.ToString();
                }
                r[2]= siteFK;

                r[3] = effort.SampleDate.ToString();
				r[4] = WktUtils.ToWkt(effort.Location as Point2<double>).ToString();
				r[5] = effort.CatchMethod;
				r[6] = effort.Strata;
				r[7] = float.IsNaN(effort.Depth) ? null : effort.Depth.ToString();
                r[8] = float.IsNaN(effort.pH) ? null : effort.pH.ToString();
				r[9] = float.IsNaN(effort.Temp) ? null : effort.Temp.ToString();
				r[10] = float.IsNaN(effort.DO) ? null : effort.DO.ToString();
				r[11] = float.IsNaN(effort.Salinity) ? null : effort.Salinity.ToString();
				r[12] = float.IsNaN(effort.Velocity) ? null : effort.Velocity.ToString();
				r[13] = effort.Description;
				r[14] = effort.IsPrivate.ToString();
				efforts.AddRow(r);
			}
			efforts.Flush();
			return;
		}

		private static void CreateHaulsCsv(CsvDb db, Dictionary<Guid, Tuple<int, NetHaulEvent>> haulDict,
			Dictionary<CompoundIdentity, Tuple<int, CatchEffort>> effortDict, Dictionary<CompoundIdentity, Instrument> netDict, UserSecurityContext ctx)
		{
			string[] cols = new string[] { "NetHaulEventKey", "CatchEffortKey", "Net", "AreaSampled", "VolumeSampled", "Description" };

			ITable hauls = db.Create("NetHauls", cols);

			var orderedHauls = haulDict.OrderBy(x => x.Value.Item1);
			foreach (var haulEvt in orderedHauls)
			{
				NetHaulEvent haul = haulEvt.Value.Item2;
				Instrument net = netDict[haul.NetId];
				IRow r = hauls.CreateRow();
				r[0] = haulEvt.Value.Item1.ToString();
				r[1] = effortDict[haul.CatchEffortId].Item1.ToString();
				r[2] = net.Name;
				r[3] = float.IsNaN(haul.AreaSampled) ? null : haul.AreaSampled.ToString();
				r[4] = float.IsNaN(haul.VolumeSampled) ? null : haul.VolumeSampled.ToString();
				r[5] = haul.Description;
				hauls.AddRow(r);
			}
			hauls.Flush();
			return;
		}

		private static void CreateCountsCsv(CsvDb db, Dictionary<CompoundIdentity, TaxaUnit> taxaDict,
			Dictionary<CompoundIdentity, Tuple<int, CatchEffort>> effortDict, Dictionary<Guid, Tuple<int, FishCount>> countDict,
			HashSet<CompoundIdentity> nativeIds)
		{
            string[] cols = new string[] { "CatchEffortKey", "Taxa", "Count", "Description", "IsNative" };

            ITable counts = db.Create("FishCounts", cols);

			var orderedCounts = countDict.OrderBy(x => x.Value.Item1);
			foreach (var cnt in orderedCounts)
			{
				FishCount count = cnt.Value.Item2;
				TaxaUnit taxa = taxaDict[count.TaxaId];
				IRow r = counts.CreateRow();
				r[0] = effortDict[count.CatchEffortId].Item1.ToString();
				r[1] = taxa.Name;
				r[2] = count.Count.ToString();
				r[3] = count.Description;
				r[4] = nativeIds.Contains(taxa.Identity) ? "TRUE" : "FALSE";
				counts.AddRow(r);
			}
			counts.Flush();
			return;
		}

		private static void CreateMetricCsv(CsvDb db, Dictionary<Guid, Tuple<int, CatchMetric>> metricDict,
			Dictionary<CompoundIdentity, Tuple<int, CatchEffort>> effortDict)
		{
            string[] cols = new string[] { "CatchEffortKey", "MetricType", "Value", "Description" };

            ITable metrics = db.Create("CatchMetrics", cols);

			var orderedMetrics = metricDict.OrderBy(x => x.Value.Item1);
			foreach (var met in orderedMetrics)
			{
				CatchMetric metric = met.Value.Item2;
				IRow r = metrics.CreateRow();
				r[0] = effortDict[metric.CatchEffortId].Item1.ToString();
				r[1] = metric.MetricType;
				r[2] = float.IsNaN(metric.Value) ? null : metric.Value.ToString();
				r[3] = metric.Description;
				metrics.AddRow(r);
			}
			metrics.Flush();
			return;
		}

		private static void CreateFishCsv(CsvDb db, Dictionary<Guid, Tuple<int, Osrs.Oncor.WellKnown.Fish.Fish>> fishDict,
			Dictionary<CompoundIdentity, TaxaUnit> taxaDict, Dictionary<CompoundIdentity, Tuple<int, CatchEffort>> effortDict,
			HashSet<CompoundIdentity> nativeIds)
		{
			string[] cols = new string[] { "FishKey", "CatchEffortKey", "Taxa", "LengthStandard", "LengthFork", "LengthTotal",
			"Weight", "AdClipped", "CWT", "Description", "IsNative" };

			ITable fishes = db.Create("Fish", cols);

			var orderedFish = fishDict.OrderBy(x => x.Value.Item1);
			foreach (var fsh in orderedFish)
			{
				var fish = fsh.Value.Item2;
				TaxaUnit taxa = taxaDict[fish.TaxaId];
				IRow r = fishes.CreateRow();
				r[0] = fsh.Value.Item1.ToString();
				r[1] = effortDict[fish.CatchEffortId].Item1.ToString();
				r[2] = taxa.Name;
				r[3] = float.IsNaN(fish.LengthStandard) ? null : fish.LengthStandard.ToString();
				r[4] = float.IsNaN(fish.LengthFork) ? null : fish.LengthFork.ToString();
				r[5] = float.IsNaN(fish.LengthTotal) ? null : fish.LengthTotal.ToString();
				r[6] = float.IsNaN(fish.Weight) ? null : fish.Weight.ToString();
				r[7] = fish.AdClipped.ToString();
				r[8] = fish.CWT.ToString();
				r[9] = fish.Description;
				r[10] = nativeIds.Contains(taxa.Identity) ? "TRUE" : "FALSE";
				fishes.AddRow(r);
			}
			fishes.Flush();
			return;
		}

		private static void CreateIdTagCsv(CsvDb db, Dictionary<Guid, Tuple<int, FishIdTag>> tagDict,
			Dictionary<Guid, Tuple<int, Osrs.Oncor.WellKnown.Fish.Fish>> fishDict)
		{
            string[] cols = new string[] { "FishKey", "TagCode", "TagType", "TagManufacturer", "Description" };

            ITable tags = db.Create("FishIdTags", cols);

			var orderedTags = tagDict.OrderBy(x => x.Value.Item1);
			foreach (var t in orderedTags)
			{
				FishIdTag tag = t.Value.Item2;
				IRow r = tags.CreateRow();
				r[0] = fishDict[tag.FishId].Item1.ToString();
				r[1] = tag.TagCode;
				r[2] = tag.TagType;
				r[3] = tag.TagManufacturer;
				r[4] = tag.Description;
				tags.AddRow(r);
			}
			tags.Flush();
			return;
		}

		private static void CreateDietCsv(CsvDb db, Dictionary<Guid, Tuple<int, Osrs.Oncor.WellKnown.Fish.Fish>> fishDict,
			Dictionary<Guid, Tuple<int, FishDiet>> dietDict, Dictionary<CompoundIdentity, TaxaUnit> taxaDict, HashSet<CompoundIdentity> nativeIds)
		{
            string[] cols = new string[] { "FishKey", "GutSampleId", "VialId", "Taxa", "Lifestage", "Count", "WholeAnimalWeight", "IndividualMass", "SampleMass", "Description", "IsNative" };

            ITable diets = db.Create("FishDiets", cols);

			var orderedDiets = dietDict.OrderBy(x => x.Value.Item1);
			foreach (var d in orderedDiets)
			{
				FishDiet diet = d.Value.Item2;

                TaxaUnit taxa = null;
                if (!diet.TaxaId.IsEmpty)
                {
                    taxa = taxaDict[diet.TaxaId];  //could be empty gut sample {species=null; count=null}
                }

                IRow r = diets.CreateRow();
				r[0] = fishDict[diet.FishId].Item1.ToString();
                r[1] = diet.GutSampleId;
                r[2] = diet.VialId;
                r[3] = taxa != null ? taxa.Name : String.Empty;
				r[4] = diet.LifeStage;
				r[5] = diet.Count != null ? diet.Count.ToString() : null;
                r[6] = diet.WholeAnimalsWeighed.ToString();
				r[7] = float.IsNaN(diet.IndividualMass) ? null : diet.IndividualMass.ToString();
                r[8] = float.IsNaN(diet.SampleMass) ? null : diet.SampleMass.ToString();
				r[9] = diet.Description;

                if (taxa == null)
                {
                    r[10] = null;
                }
                else
                {
                    if (nativeIds.Contains(taxa.Identity))
                        r[10] = "TRUE";
                    else
                        r[10] = "FALSE";
                }

                diets.AddRow(r);
			}
			diets.Flush();
			return;
		}

		private static void CreateGeneticsCsv(CsvDb db, Dictionary<Guid, Tuple<int, FishGenetics>> geneDict,
			Dictionary<Guid, Tuple<int, Osrs.Oncor.WellKnown.Fish.Fish>> fishDict)
		{
            string[] cols = new string[] { "FishKey", "GeneticSampleId", "LabSampleId", "BestStockEstimate", "ProbabilityBest", "SecondStockEstimate", "ProbabilitySecondBest", "ThirdStockEstimate", "ProbabilityThirdBest", "Description" };

            ITable genes = db.Create("FishGenetics", cols);

			var orderedGenes = geneDict.OrderBy(x => x.Value.Item1);
			foreach (var g in orderedGenes)
			{
				FishGenetics gene = g.Value.Item2;
				IRow r = genes.CreateRow();				
				r[0] = fishDict[gene.FishId].Item1.ToString();
				r[1] = gene.GeneticSampleId;
				r[2] = gene.LabSampleId;

                //stock estimates must fill 6 columns (3 pair)
                int idx = 3;        
                foreach (var est in gene.StockEstimates)
                {
                    r[idx] = est.Stock.ToString();  //estimate
                    idx++;
                    r[idx] = float.IsNaN(est.Probability) ? null : est.Probability.ToString();    //probability
                    idx++;
                }
                if (gene.StockEstimates.Count < 3)
                {
                    int emptyCols = (3 - gene.StockEstimates.Count);
                    for (int i = 0; i < emptyCols; i++)
                    {
                        r[idx] = null;    //empty estimate
                        idx++;
                        r[idx] = null;    //empty probability
                        idx++;
                    }
                }

                r[idx] = gene.Description;
				genes.AddRow(r);
			}
			genes.Flush();
			return;
		}

		private static void CreateSitesCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict)
		{
			string[] cols = new string[] { "SiteKey", "Name", "Description", "Location", "LocationMark" };

			ITable sites = db.Create("Sites", cols);

			var orderedSites = siteDict.OrderBy(x => x.Value.Item1);
			foreach (var site in orderedSites)
			{
				Site s = site.Value.Item2;
				IRow r = sites.CreateRow();
				r[0] = site.Value.Item1.ToString();
				r[1] = s.Name;
				r[2] = s.Description;
				IGeometry2<double> geom = s.Location;
				if (geom != null)
				{
					if (geom is PolygonBag2<double>)
						r[3] = WktUtils.ToWkt(geom as PolygonBag2<double>).ToString();
					else if (geom is Polygon2<double>)
						r[3] = WktUtils.ToWkt(geom as Polygon2<double>).ToString();
					else if (geom is Polyline2<double>)
						r[3] = WktUtils.ToWkt(geom as Polyline2<double>).ToString();
					else if (geom is PolylineBag2<double>)
						r[3] = WktUtils.ToWkt(geom as PolylineBag2<double>).ToString();
					else if (geom is Point2<double>)
						r[3] = WktUtils.ToWkt(geom as Point2<double>).ToString();
				}
				else
				{
					r[3] = "";
				}

				Point2<double> geom2 = s.LocationMark;
				if (geom2 != null)
					r[4] = WktUtils.ToWkt(geom2 as Point2<double>).ToString();
				else
					r[4] = "";

				sites.AddRow(r);
			}
			sites.Flush();
			return;
		}

		private Guid CreateExcelFile(Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict, Dictionary<CompoundIdentity, Tuple<int, CatchEffort>> effortDict,
			Dictionary<Guid, Tuple<int, NetHaulEvent>> haulDict, Dictionary<Guid, Tuple<int, FishCount>> countDict, Dictionary<Guid, Tuple<int, CatchMetric>> metricDict,
			Dictionary<Guid, Tuple<int, Osrs.Oncor.WellKnown.Fish.Fish>> fishDict, Dictionary<Guid, Tuple<int, FishIdTag>> tagDict, Dictionary<Guid, Tuple<int, FishDiet>> dietDict,
			Dictionary<Guid, Tuple<int, FishGenetics>> geneDict, Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict, Dictionary<CompoundIdentity, Organization> orgDict,
			Dictionary<CompoundIdentity, FieldTrip> fieldTripDict, Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict, Dictionary<CompoundIdentity, Project> projectDict,
			Dictionary<CompoundIdentity, TaxaUnit> taxaDict, Dictionary<CompoundIdentity, Instrument> netDict, HashSet<CompoundIdentity> nativeIds, UserSecurityContext ctx)
		{
			IFileStoreProvider provider = FileStoreManager.Instance.GetProvider();
			FilestoreFile fishFile = provider.MakeTemp(DateTime.UtcNow.AddHours(4));
			XlWorkbook book = new XlWorkbook();
			XlWorksheets sheets = book.Worksheets;

			XlSchema upperSchema = GetUpperSchema();
			XlWorksheet upperSheet = sheets.AddWorksheet("SamplingEvents", XlColor.White, upperSchema);
			XlRows upperRows = upperSheet.Rows;

			var orderedEvents = eventDict.OrderBy(x => x.Value.Item1);
			foreach (var evt in orderedEvents)
			{
				string orgName = na;
				if (orgDict.ContainsKey(evt.Value.Item2.PrincipalOrgId))
					orgName = orgDict[evt.Value.Item2.PrincipalOrgId].Name;
				string ftripName = na;
				string factivityName = na;
				string projName = na;
				if (fieldTripDict.ContainsKey(evt.Value.Item2.FieldTripId))
				{
					FieldTrip ftrip = fieldTripDict[evt.Value.Item2.FieldTripId];
					ftripName = ftrip.Name;
					if (fieldActivityDict.ContainsKey(ftrip.FieldActivityId))
					{
						FieldActivity factivity = fieldActivityDict[ftrip.FieldActivityId];
						factivityName = factivity.Name;
						if (projectDict.ContainsKey(factivity.ProjectId))
							projName = projectDict[factivity.ProjectId].Name;
					}
				}
				
				List<string> evtItems = new List<string>();
				evtItems.Add(orgName);
				evtItems.Add(ftripName);
				evtItems.Add(factivityName);
				evtItems.Add(projName);
				evtItems.Add(evt.Value.Item2.Name);
				evtItems.Add(evt.Value.Item1.ToString());
				evtItems.Add(evt.Value.Item2.Description);

				SchemaRowData row = new SchemaRowData(upperSchema, evtItems);
				upperRows.AddRow(row);
			}

			XlSchema effortSchema = GetFishCatchEffortSchema();
			XlWorksheet effortSheet = sheets.AddWorksheet("CatchEfforts", XlColor.White, effortSchema);
			XlRows effortRows = effortSheet.Rows;

			var orderedEfforts = effortDict.OrderBy(x => x.Value.Item1);
			foreach (var eff in orderedEfforts)
			{
				CatchEffort effort = eff.Value.Item2;
				List<string> effItems = new List<string>();
				effItems.Add(eff.Value.Item1.ToString());
				effItems.Add(eventDict[effort.SampleEventId].Item1.ToString());

                //effort.SiteId could be a dangling reference
                string siteFK = na;
                if (siteDict.ContainsKey(effort.SiteId))
                {
                    siteFK = siteDict[effort.SiteId].Item1.ToString();
                }
                effItems.Add(siteFK);

                effItems.Add(effort.SampleDate.ToString());
				effItems.Add(WktUtils.ToWkt(effort.Location as Point2<double>).ToString());
				effItems.Add(effort.CatchMethod);
				effItems.Add(effort.Strata);
				effItems.Add(float.IsNaN(effort.Depth) ? null : effort.Depth.ToString());
				effItems.Add(float.IsNaN(effort.pH) ? null : effort.pH.ToString());
				effItems.Add(float.IsNaN(effort.Temp) ? null : effort.Temp.ToString());
				effItems.Add(float.IsNaN(effort.DO) ? null : effort.DO.ToString());
				effItems.Add(float.IsNaN(effort.Salinity) ? null : effort.Salinity.ToString());
				effItems.Add(float.IsNaN(effort.Velocity) ? null : effort.Velocity.ToString());
				effItems.Add(effort.Description);
				effItems.Add(effort.IsPrivate.ToString());

				SchemaRowData row = new SchemaRowData(effortSchema, effItems);
				effortRows.AddRow(row);
			}

			XlSchema haulSchema = GetFishNetHaulEventSchema();
			XlWorksheet haulSheet = sheets.AddWorksheet("NetHauls", XlColor.White, haulSchema);
			XlRows haulRows = haulSheet.Rows;

			var orderedHauls = haulDict.OrderBy(x => x.Value.Item1);
			foreach (var haulEvt in orderedHauls)
			{
				NetHaulEvent haul = haulEvt.Value.Item2;
				Instrument net = netDict[haul.NetId];
				List<string> haulItems = new List<string>();
				haulItems.Add(haulEvt.Value.Item1.ToString());
				haulItems.Add(effortDict[haul.CatchEffortId].Item1.ToString());
				haulItems.Add(net.Name);
                haulItems.Add(float.IsNaN(haul.AreaSampled) ? null : haul.AreaSampled.ToString());
				haulItems.Add(float.IsNaN(haul.VolumeSampled) ? null : haul.VolumeSampled.ToString());
				haulItems.Add(haul.Description);

				SchemaRowData row = new SchemaRowData(haulSchema, haulItems);
				haulRows.AddRow(row);
			}

			XlSchema countSchema = GetFishCountSchema();
			XlWorksheet countSheet = sheets.AddWorksheet("FishCounts", XlColor.White, countSchema);
			XlRows countRows = countSheet.Rows;

			var orderedCounts = countDict.OrderBy(x => x.Value.Item1);
			foreach (var cnt in orderedCounts)
			{
				FishCount count = cnt.Value.Item2;
				TaxaUnit taxa = taxaDict[count.TaxaId];
				List<string> countItems = new List<string>();
				countItems.Add(effortDict[count.CatchEffortId].Item1.ToString());
				countItems.Add(taxa.Name);
				countItems.Add(count.Count.ToString());
				countItems.Add(count.Description);
				countItems.Add(nativeIds.Contains(taxa.Identity) ? "TRUE" : "FALSE");

				SchemaRowData row = new SchemaRowData(countSchema, countItems);
				countRows.AddRow(row);
			}

			XlSchema metricSchema = GetCatchMetricSchema();
			XlWorksheet metricSheet = sheets.AddWorksheet("CatchMetrics", XlColor.White, metricSchema);
			XlRows metricRows = metricSheet.Rows;

			var orderedMetrics = metricDict.OrderBy(x => x.Value.Item1);
			foreach (var met in orderedMetrics)
			{
				CatchMetric metric = met.Value.Item2;
				List<string> metricItems = new List<string>();
				metricItems.Add(effortDict[metric.CatchEffortId].Item1.ToString());
				metricItems.Add(metric.MetricType);
				metricItems.Add(float.IsNaN(metric.Value) ? null : metric.Value.ToString());
				metricItems.Add(metric.Description);

				SchemaRowData row = new SchemaRowData(metricSchema, metricItems);
				metricRows.AddRow(row);
			}

			XlSchema fishSchema = GetFishSchema();
			XlWorksheet fishSheet = sheets.AddWorksheet("Fish", XlColor.White, fishSchema);
			XlRows fishRows = fishSheet.Rows;

			var orderedFish = fishDict.OrderBy(x => x.Value.Item1);
			foreach (var fsh in orderedFish)
			{
				var fish = fsh.Value.Item2;
				TaxaUnit taxa = taxaDict[fish.TaxaId];
				List<string> fishItems = new List<string>();
				fishItems.Add(fsh.Value.Item1.ToString());
				fishItems.Add(effortDict[fish.CatchEffortId].Item1.ToString());
				fishItems.Add(taxa.Name);
				fishItems.Add(float.IsNaN(fish.LengthStandard) ? null : fish.LengthStandard.ToString());
				fishItems.Add(float.IsNaN(fish.LengthFork) ? null : fish.LengthFork.ToString());
				fishItems.Add(float.IsNaN(fish.LengthTotal) ? null : fish.LengthTotal.ToString());
				fishItems.Add(float.IsNaN(fish.Weight) ? null : fish.Weight.ToString());
				fishItems.Add(fish.AdClipped.ToString());
				fishItems.Add(fish.CWT.ToString());
				fishItems.Add(fish.Description);
				fishItems.Add(nativeIds.Contains(taxa.Identity) ? "TRUE" : "FALSE");

				SchemaRowData row = new SchemaRowData(fishSchema, fishItems);
				fishRows.AddRow(row);
			}

			XlSchema tagSchema = GetFishIdTagSchema();
			XlWorksheet tagSheet = sheets.AddWorksheet("FishIdTags", XlColor.White, tagSchema);
			XlRows tagRows = tagSheet.Rows;

			var orderedTags = tagDict.OrderBy(x => x.Value.Item1);
			foreach (var t in orderedTags)
			{
				FishIdTag tag = t.Value.Item2;
				List<string> tagItems = new List<string>();
				//tagItems.Add(t.Value.Item1.ToString());
				tagItems.Add(fishDict[tag.FishId].Item1.ToString());
				tagItems.Add(tag.TagCode);
				tagItems.Add(tag.TagType);
				tagItems.Add(tag.TagManufacturer);
				tagItems.Add(tag.Description);

				SchemaRowData row = new SchemaRowData(tagSchema, tagItems);
				tagRows.AddRow(row);
			}

			XlSchema dietSchema = GetFishDietSchema();
			XlWorksheet dietSheet = sheets.AddWorksheet("FishDiets", XlColor.White, dietSchema);
			XlRows dietRows = dietSheet.Rows;

			var orderedDiets = dietDict.OrderBy(x => x.Value.Item1);
			foreach (var d in orderedDiets)
			{
				FishDiet diet = d.Value.Item2;

                TaxaUnit taxa = null;
                if (!diet.TaxaId.IsEmpty)
                {
                    taxa = taxaDict[diet.TaxaId];  //could be empty gut sample {species=null; count=null}
                }
				
				List<string> dietItems = new List<string>();

				dietItems.Add(fishDict[diet.FishId].Item1.ToString());
                dietItems.Add(diet.GutSampleId);
                dietItems.Add(diet.VialId.ToString());
                dietItems.Add(taxa != null ? taxa.Name : String.Empty);
                dietItems.Add(diet.LifeStage);
				dietItems.Add(diet.Count != null ? diet.Count.ToString() : null);
				dietItems.Add(diet.WholeAnimalsWeighed.ToString());
				dietItems.Add(float.IsNaN(diet.IndividualMass) ? null : diet.IndividualMass.ToString());
				dietItems.Add(float.IsNaN(diet.SampleMass) ? null : diet.SampleMass.ToString());
				dietItems.Add(diet.Description);

                if (taxa == null)
                {
                    dietItems.Add(null);
                }
                else
                {
                    if (nativeIds.Contains(taxa.Identity))
                        dietItems.Add("TRUE");
                    else
                        dietItems.Add(null);
                }

				SchemaRowData row = new SchemaRowData(dietSchema, dietItems);
				dietRows.AddRow(row);
			}

			XlSchema geneSchema = GetFishGeneticsSchema();
			XlWorksheet geneSheet = sheets.AddWorksheet("FishGenetics", XlColor.White, geneSchema);
			XlRows geneRows = geneSheet.Rows;

			var orderedGenes = geneDict.OrderBy(x => x.Value.Item1);
            foreach (var g in orderedGenes)
            {
                FishGenetics gene = g.Value.Item2;
                List<string> geneItems = new List<string>();
                geneItems.Add(fishDict[gene.FishId].Item1.ToString());
                geneItems.Add(gene.GeneticSampleId);
                geneItems.Add(gene.LabSampleId);

                //stock estimates must fill 6 columns (3 pair)
                foreach (var est in gene.StockEstimates)
                {
                    geneItems.Add(est.Stock.ToString());                                                //estimate
                    geneItems.Add(float.IsNaN(est.Probability) ? null : est.Probability.ToString());    //probability
                }
                if (gene.StockEstimates.Count < 3)
                {
                    int emptyCols = (3 - gene.StockEstimates.Count);
                    for (int i = 0; i < emptyCols; i++)
                    {
                        geneItems.Add(null);    //empty estimate
                        geneItems.Add(null);    //empty probability
                    }
                }

                geneItems.Add(gene.Description);

				SchemaRowData row = new SchemaRowData(geneSchema, geneItems);
				geneRows.AddRow(row);
			}

			//Generating Site Sheet
			XlSchema siteSchema = GetSiteSchema();
			XlWorksheet siteSheet = sheets.AddWorksheet("Sites", XlColor.White, siteSchema);
			XlRows siteRows = siteSheet.Rows;

			var orderedSites = siteDict.OrderBy(x => x.Value.Item1);
			foreach (var site in orderedSites)
			{
				Site s = site.Value.Item2;
				List<string> siteItems = new List<string>();
				siteItems.Add(site.Value.Item1.ToString());
				siteItems.Add(s.Name);
				siteItems.Add(s.Description);
				IGeometry2<double> geom = s.Location;
				if (geom != null)
				{
					if (geom is PolygonBag2<double>)
						siteItems.Add(WktUtils.ToWkt(geom as PolygonBag2<double>).ToString());
					else if (geom is Polygon2<double>)
						siteItems.Add(WktUtils.ToWkt(geom as Polygon2<double>).ToString());
					else if (geom is Polyline2<double>)
						siteItems.Add(WktUtils.ToWkt(geom as Polyline2<double>).ToString());
					else if (geom is PolylineBag2<double>)
						siteItems.Add(WktUtils.ToWkt(geom as PolylineBag2<double>).ToString());
					else if (geom is Point2<double>)
						siteItems.Add(WktUtils.ToWkt(geom as Point2<double>).ToString());
				}
				else
				{
					siteItems.Add("");
				}

				Point2<double> geom2 = s.LocationMark;
				if (geom2 != null)
					siteItems.Add(WktUtils.ToWkt(geom2 as Point2<double>).ToString());
				else
					siteItems.Add("");

				SchemaRowData row = new SchemaRowData(siteSchema, siteItems);
				siteRows.AddRow(row);
			}

			book.Save(fishFile);
			fishFile.Flush();
			fishFile.Close();
			fishFile.Dispose();
			return fishFile.FileId;
		}

		private static XlSchema GetUpperSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("Org", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("FieldTrip", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Activity", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Project", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("SampleEvent", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("SampleEventKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("SampleEventDesc", typeof(string), StyleSheetHelper.Normal);			
			return schema;
		}

		private static XlSchema GetFishCatchEffortSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("CatchEffortKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("SamplingEventKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("SiteKey", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Time", typeof(DateTime), StyleSheetHelper.Normal);
			schema.AddColumn("AdHocLocation", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Method", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Strata", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Depth", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("pH", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("Temp", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("DO", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("Salinity", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("Velocity", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsPrivate", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetFishNetHaulEventSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("NetHaulEventKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("CatchEffortKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("Net", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("AreaSampled", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("VolumeSampled", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetFishCountSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("CatchEffortKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("Taxa", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Count", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsNative", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetCatchMetricSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("CatchEffortKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("MetricType", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Value", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetFishSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("FishKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("CatchEffortKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("Taxa", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("LengthStandard", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("LengthFork", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("LengthTotal", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("Weight", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("AdClipped", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("CWT", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsNative", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetFishIdTagSchema()
		{
			XlSchema schema = new XlSchema();
			//schema.AddColumn("TagKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("FishKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("TagCode", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("TagType", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("TagManufacturer", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetFishDietSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("FishKey", typeof(int), StyleSheetHelper.Normal);
            schema.AddColumn("GutSampleId", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("VialId", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("Taxa", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("Lifestage", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Count", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("WholeAnimalsWeighed", typeof(uint), StyleSheetHelper.Normal);
			schema.AddColumn("IndividualMass", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("SampleMass", typeof(float), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsNative", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetFishGeneticsSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("FishKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("GeneticSampleId", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("LabSampleId", typeof(string), StyleSheetHelper.Normal);            
            schema.AddColumn("BestStockEstimate", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ProbabilityBest", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("SecondStockEstimate", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ProbabilitySecondBest", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ThirdStockEstimate", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ProbabilityThirdBest", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetSiteSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("SiteKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("Name", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Location", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("LocationMark", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private class SchemaRowData : IXlRowData
		{
			private readonly List<IXlCell> _list = new List<IXlCell>();
			public SchemaRowData(XlSchema schema, List<string> items)
			{
				int columnIndex = 0;
				foreach (XlColumn column in schema.Columns)
				{
					string item;
					if (items[columnIndex] != null)
						item = items[columnIndex];
					else
						item = "";
					XlCell cell = new XlCell(0U, column.Type, item);
					_list.Add(cell);
					columnIndex++;
				}
			}

			public IEnumerator<IXlCell> GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _list.GetEnumerator();
			}
		}
	}
}
