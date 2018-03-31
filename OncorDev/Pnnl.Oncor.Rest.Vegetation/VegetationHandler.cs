using Osrs.Net.Http.Handlers;
using Osrs.Security.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osrs.Net.Http;
using Osrs.Threading;
using Osrs.Security;
using Osrs.Data;
using Newtonsoft.Json.Linq;
using Osrs.Oncor.WellKnown.Vegetation;
using Osrs.Oncor.WellKnown.Vegetation.Module;
using Osrs.WellKnown.FieldActivities;
using Osrs.WellKnown.Organizations;
using Osrs.WellKnown.Projects;
using Osrs.WellKnown.Sites;
using Osrs.Oncor.Excel;
using System.Collections;
using Osrs.Oncor.EntityBundles;
using Osrs.WellKnown.Taxonomy;
using Osrs.Oncor.FileStore;
using Osrs.Oncor.SimpleDb;
using Osrs.Numerics.Spatial.Geometry;

namespace Pnnl.Oncor.Rest.Vegetation
{
	public sealed class VegetationHandler : HttpHandlerBase, IServiceHandler
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
				return "veg";
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
							if (meth.Equals("survey", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									HashSet<CompoundIdentity> ids = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["surveyids"] != null)
											ids = JsonUtils.ToIds(token["surveyids"]);
									}

									IEnumerable<VegSurvey> surveys = GetVegSurveys(ctx, ids);
									JArray jsurveys = Jsonifier.ToJson(surveys);
									if (jsurveys != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jsurveys.ToString());
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
							else if (meth.Equals("plottype", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									HashSet<CompoundIdentity> ids = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["plotids"] != null)
											ids = JsonUtils.ToIds(token["plotids"]);
									}

									IEnumerable<VegPlotType> plots = GetVegPlots(ctx, ids);
									JArray jplots = Jsonifier.ToJson(plots);
									if (jplots != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jplots.ToString());
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
							else if (meth.Equals("sample", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									HashSet<Guid> ids = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["sampleids"] != null)
											ids = JsonUtils.ToGuids(token["sampleids"]);
									}

									IEnumerable<VegSample> samples = GetVegSamples(ctx, ids);
									JArray jsamples = Jsonifier.ToJson(samples);
									if (jsamples != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jsamples.ToString());
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
							else if (meth.Equals("herb", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									HashSet<Guid> ids = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["herbsampleids"] != null)
											ids = JsonUtils.ToGuids(token["herbsampleids"]);
									}

									IEnumerable<HerbSample> herbs = GetHerbSamples(ctx, ids);
									JArray jherbs = Jsonifier.ToJson(herbs);
									if (jherbs != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jherbs.ToString());
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
							else if (meth.Equals("shrub", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									HashSet<Guid> ids = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["shrubsampleids"] != null)
											ids = JsonUtils.ToGuids(token["shrubsampleids"]);
									}

									IEnumerable<ShrubSample> shrubs = GetShrubSamples(ctx, ids);
									JArray jshrubs = Jsonifier.ToJson(shrubs);
									if (jshrubs != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jshrubs.ToString());
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
							else if (meth.Equals("tree", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									HashSet<Guid> ids = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["treesampleids"] != null)
											ids = JsonUtils.ToGuids(token["treesampleids"]);
									}

									IEnumerable<TreeSample> trees = GetTreeSamples(ctx, ids);
									JArray jtrees = Jsonifier.ToJson(trees);
									if (jtrees != null)
									{
										RestUtils.Push(context.Response, JsonOpStatus.Ok, jtrees.ToString());
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
										HashSet<CompoundIdentity> vegSurveyIds = null;
										HashSet<CompoundIdentity> siteIds = null;
                                        Guid nativeBundleId;
                                        Guid wetlandBundleId;

										DateTime? start = null;
										DateTime? end = null;

										JToken token = JsonUtils.GetDataPayload(context.Request);
										if (token != null)
										{
											if (token["events"] != null)
												eventIds = JsonUtils.ToIds(token["events"]);

											if (token["surveys"] != null)
												vegSurveyIds = JsonUtils.ToIds(token["surveys"]);

											if (token["sites"] != null)
												siteIds = JsonUtils.ToIds(token["sites"]);

											if (token["start"] != null)
												start = JsonUtils.ToDate(token["start"]);

											if (token["end"] != null)
												end = JsonUtils.ToDate(token["end"]);

                                            if (token["nativeBundleId"] != null)
                                                nativeBundleId = JsonUtils.ToGuid(token["nativeBundleId"]);

											if (token["wetlandBundleId"] != null)
                                                wetlandBundleId = JsonUtils.ToGuid(token["wetlandBundleId"]);
                                        }

										IEnumerable<VegSurvey> surveys = GetVegSurveys(ctx, vegSurveyIds);
										if (eventIds != null)
											surveys = surveys.Where(x => eventIds.Contains(x.SampleEventId));
										if (siteIds != null)
											surveys = surveys.Where(x => siteIds.Contains(x.SiteId));

										IEnumerable<VegSurvey> filteredSurveys = surveys.ToList();

										//Get sampling events
										List<CompoundIdentity> selectedEvents = filteredSurveys.Select(x => x.SampleEventId).ToList();
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

										IEnumerable<CompoundIdentity> surveyIds = filteredSurveys.Select(x => x.Identity);
										IEnumerable<CompoundIdentity> plotIds = filteredSurveys.Select(x => x.PlotTypeId);
										IEnumerable<VegPlotType> plots = GetVegPlots(ctx, plotIds);
										IEnumerable<VegSample> vegSamples = GetVegSampleBySurvey(ctx, surveyIds);
										IEnumerable<Guid> vegIds = vegSamples.Select(x => x.Identity);
										IEnumerable<HerbSample> herbs = GetHerbsByVegSample(ctx, vegIds);
										IEnumerable<ShrubSample> shrubs = GetShrubsByVegSample(ctx, vegIds);
										IEnumerable<TreeSample> trees = GetTreesByVegSample(ctx, vegIds);

                                        //Get sites (those attached to the survey and any vegSamples; so we have a complete dictionary
                                        List<CompoundIdentity> siteIdsList = new List<CompoundIdentity>();

                                        //List<CompoundIdentity> selectedSites = filteredSurveys.Select(x => x.SiteId).ToList();  //sites on the surveys
                                        foreach (VegSurvey v in filteredSurveys)
                                        {
                                            if (!v.SiteId.IsEmpty)
                                                if(!siteIdsList.Contains(v.SiteId))
                                                    siteIdsList.Add(v.SiteId);
                                        }
                                        
                                        //List<CompoundIdentity> sampleSites = vegSamples.Select(x => x.SiteId).ToList(); //sample sites
                                        foreach (VegSample v in vegSamples)
                                        {
                                            if (!v.SiteId.IsEmpty)
                                                if (!siteIdsList.Contains(v.SiteId))
                                                    siteIdsList.Add(v.SiteId);
                                        }

                                        IEnumerable<Site> sitesData = GetSites(ctx, siteIdsList);

                                        Guid fileId = CreateVegFile(sitesData, eventsData, orgData, fieldTripData, fieldActivityData, projectData, filteredSurveys, plots, vegSamples, herbs, shrubs, trees, nativeBundleId, wetlandBundleId, ctx);

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

		private IEnumerable<VegSurvey> GetVegSurveys(UserSecurityContext ctx, IEnumerable<CompoundIdentity> ids)
		{
			IVegSurveyProvider survProv = VegetationManager.Instance.GetSurveyProvider(ctx);
			if (survProv != null)
			{
				if (ids != null)
					return survProv.Get(ids);
				else
					return survProv.GetSurvey();
			}
			return null;
		}

		private IEnumerable<VegPlotType> GetVegPlots(UserSecurityContext ctx, IEnumerable<CompoundIdentity> ids)
		{
			IVegSurveyProvider survProv = VegetationManager.Instance.GetSurveyProvider(ctx);
			if (survProv != null)
			{
				if (ids != null)
					return survProv.GetPlotType(ids);
			}
			return null;
		}

		private IEnumerable<VegSample> GetVegSamples(UserSecurityContext ctx, IEnumerable<Guid> ids)
		{
			IVegSampleProvider sampProv = VegetationManager.Instance.GetSampleProvider(ctx);
			if (sampProv != null)
			{
				if (ids != null)
					return sampProv.Get(ids);
				else
					return sampProv.Get();
			}
			return null;
		}

		private IEnumerable<VegSample> GetVegSampleBySurvey(UserSecurityContext ctx, IEnumerable<CompoundIdentity> ids)
		{
			IVegSampleProvider sampProv = VegetationManager.Instance.GetSampleProvider(ctx);
			if (sampProv != null)
			{
				return sampProv.GetForSurvey(ids);
			}
			return null;
		}

		private IEnumerable<HerbSample> GetHerbSamples(UserSecurityContext ctx, IEnumerable<Guid> ids)
		{
			IVegSampleProvider sampProv = VegetationManager.Instance.GetSampleProvider(ctx);
			if (sampProv != null)
			{
				if (ids != null)
					return sampProv.GetHerb(ids);
				else
					return sampProv.GetHerb();
			}
			return null;
		}

		private IEnumerable<HerbSample> GetHerbsByVegSample(UserSecurityContext ctx, IEnumerable<Guid>ids)
		{
			IVegSampleProvider sampProv = VegetationManager.Instance.GetSampleProvider(ctx);
			if (sampProv != null && ids.Count() > 0)
			{
				return sampProv.GetHerbForVegSample(ids);
			}
			return new List<HerbSample>();
		}

		private IEnumerable<ShrubSample> GetShrubSamples(UserSecurityContext ctx, IEnumerable<Guid> ids)
		{
			IVegSampleProvider sampProv = VegetationManager.Instance.GetSampleProvider(ctx);
			if (sampProv != null)
			{
				if (ids != null)
					return sampProv.GetShrub(ids);
				else
					return sampProv.GetShrub();
			}
			return null;
		}

		private IEnumerable<ShrubSample> GetShrubsByVegSample(UserSecurityContext ctx, IEnumerable<Guid> ids)
		{
			IVegSampleProvider sampProv = VegetationManager.Instance.GetSampleProvider(ctx);
			if (sampProv != null && ids.Count() > 0)
			{
				return sampProv.GetShrubForVegSample(ids);
			}
			return new List<ShrubSample>();
		}

		private IEnumerable<TreeSample> GetTreeSamples(UserSecurityContext ctx, IEnumerable<Guid> ids)
		{
			IVegSampleProvider sampProv = VegetationManager.Instance.GetSampleProvider(ctx);
			if (sampProv != null)
			{
				if (ids != null)
					return sampProv.GetTree(ids);
				else
					return sampProv.GetTree();
			}
			return null;
		}

		private IEnumerable<TreeSample> GetTreesByVegSample(UserSecurityContext ctx, IEnumerable<Guid> ids)
		{
			IVegSampleProvider sampProv = VegetationManager.Instance.GetSampleProvider(ctx);
			if (sampProv != null && ids.Count() > 0)
			{
				return sampProv.GetTreeForVegSample(ids);
			}
			return new List<TreeSample>();
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

		private Guid CreateVegFile(IEnumerable<Site> sites, IEnumerable<SamplingEvent> events, IEnumerable<Organization> orgs,
			IEnumerable<FieldTrip> trips, IEnumerable<FieldActivity> activities, IEnumerable<Project> projects,
			IEnumerable<VegSurvey> surveys, IEnumerable<VegPlotType> plots, IEnumerable<VegSample> vegs, IEnumerable<HerbSample> herbs,
			IEnumerable<ShrubSample> shrubs, IEnumerable<TreeSample> trees, Guid nativeBundleId, Guid wetlandBundleId,
			UserSecurityContext ctx)
		{
			int i = 1;
			Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict = new Dictionary<CompoundIdentity, Tuple<int, Site>>();
			foreach (Site site in sites)
			{
				if (!siteDict.ContainsKey(site.Identity))
				{
					siteDict.Add(site.Identity, new Tuple<int, Site>(i, site));
					i++;
				}
			}

			i = 1;
			Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict = new Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>>();
			foreach (SamplingEvent samp in events)
			{
				if (!eventDict.ContainsKey(samp.Identity))
				{
					eventDict.Add(samp.Identity, new Tuple<int, SamplingEvent>(i, samp));
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

			i = 1;
			Dictionary<CompoundIdentity, Tuple<int, VegSurvey>> surveyDict = new Dictionary<CompoundIdentity, Tuple<int, VegSurvey>>();
			foreach(VegSurvey surv in surveys)
			{
				if (!surveyDict.ContainsKey(surv.Identity))
				{
					surveyDict.Add(surv.Identity, new Tuple<int, VegSurvey>(i, surv));
					i++;
				}
			}

			i = 1;
			Dictionary<CompoundIdentity, Tuple<int, VegPlotType>> plotDict = new Dictionary<CompoundIdentity, Tuple<int, VegPlotType>>();
			foreach(VegPlotType plot in plots)
			{
				if (!plotDict.ContainsKey(plot.Identity))
				{
					plotDict.Add(plot.Identity, new Tuple<int, VegPlotType>(i, plot));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, VegSample>> vegsDict = new Dictionary<Guid, Tuple<int, VegSample>>();
			foreach(VegSample veg in vegs)
			{
				if (!vegsDict.ContainsKey(veg.Identity))
				{
					vegsDict.Add(veg.Identity, new Tuple<int, VegSample>(i, veg));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, HerbSample>> herbDict = new Dictionary<Guid, Tuple<int, HerbSample>>();
			foreach(HerbSample herb in herbs)
			{
				if (!herbDict.ContainsKey(herb.Identity))
				{
					herbDict.Add(herb.Identity, new Tuple<int, HerbSample>(i, herb));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, ShrubSample>> shrubDict = new Dictionary<Guid, Tuple<int, ShrubSample>>();
			foreach(ShrubSample shrub in shrubs)
			{
				if (!shrubDict.ContainsKey(shrub.Identity))
				{
					shrubDict.Add(shrub.Identity, new Tuple<int, ShrubSample>(i, shrub));
					i++;
				}
			}

			i = 1;
			Dictionary<Guid, Tuple<int, TreeSample>> treeDict = new Dictionary<Guid, Tuple<int, TreeSample>>();
			foreach(TreeSample tree in trees)
			{
				if (!treeDict.ContainsKey(tree.Identity))
				{
					treeDict.Add(tree.Identity, new Tuple<int, TreeSample>(i, tree));
					i++;
				}
			}

			EntityBundleProvider entProv = EntityBundleManager.Instance.GetProvider(ctx);          
            EntityBundle nativeBundle = null;
            EntityBundle wetlandBundle = null;

            if (entProv != null)
			{
				if (nativeBundleId != null)
                    nativeBundle = entProv.Get(nativeBundleId);

                if (wetlandBundleId != null)
                    wetlandBundle = entProv.Get(wetlandBundleId);
            }

			List<CompoundIdentity> taxaIds = new List<CompoundIdentity>();
			taxaIds.AddRange(herbDict.Select(x => x.Value.Item2.TaxaUnitId));
			taxaIds.AddRange(shrubDict.Select(x => x.Value.Item2.TaxaUnitId));
			taxaIds.AddRange(treeDict.Select(x => x.Value.Item2.TaxaUnitId));
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

			int entryCount = siteDict.Count + eventDict.Count + orgDict.Count + fieldTripDict.Count + fieldActivityDict.Count + projectDict.Count +
				surveyDict.Count + plotDict.Count + vegsDict.Count + herbDict.Count + shrubDict.Count + treeDict.Count;

			if (entryCount <= excelDataCutoff)
			{
				fileExtension = "xlsx";
				return CreateExcelFile(siteDict, eventDict, orgDict, fieldTripDict, fieldActivityDict, projectDict, surveyDict, plotDict, vegsDict,
					herbDict, shrubDict, treeDict, taxaDict, nativeBundle, wetlandBundle);
			}
			else
			{
				fileExtension = "zip";
				return CreateCsvFile(siteDict, eventDict, orgDict, fieldTripDict, fieldActivityDict, projectDict, surveyDict, plotDict, vegsDict,
					herbDict, shrubDict, treeDict, taxaDict, nativeBundle, wetlandBundle);
			}
		}

		private Guid CreateCsvFile(Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict, Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict,
			Dictionary<CompoundIdentity, Organization> orgDict, Dictionary<CompoundIdentity, FieldTrip> fieldTripDict, Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict,
			Dictionary<CompoundIdentity, Project> projectDict, Dictionary<CompoundIdentity, Tuple<int, VegSurvey>> surveyDict, Dictionary<CompoundIdentity, Tuple<int, VegPlotType>> plotDict,
			Dictionary<Guid, Tuple<int, VegSample>> vegsDict, Dictionary<Guid, Tuple<int, HerbSample>> herbDict, Dictionary<Guid, Tuple<int, ShrubSample>> shrubDict,
			Dictionary<Guid, Tuple<int, TreeSample>> treeDict, Dictionary<CompoundIdentity, TaxaUnit> taxaDict, EntityBundle nativeBundle, EntityBundle wetlandBundle) 
		{
			IFileStoreProvider provider = FileStoreManager.Instance.GetProvider();

			//Setting up temp file
			FilestoreFile vegFile = provider.MakeTemp(DateTime.UtcNow.AddHours(4));
			CsvDb csv = CsvDb.Create(vegFile);

			CreateUpperCsv(csv, eventDict, orgDict, fieldTripDict, fieldActivityDict, projectDict);
			CreateVegSurveyCsv(csv, eventDict, surveyDict, siteDict, plotDict);			
            CreateHerbSampleCsv(csv, surveyDict, siteDict, herbDict, vegsDict, taxaDict, nativeBundle, wetlandBundle);
            CreateShrubSampleCsv(csv, surveyDict, siteDict, shrubDict, vegsDict, taxaDict, nativeBundle, wetlandBundle);
            CreateTreeSampleCsv(csv, surveyDict, siteDict, treeDict, vegsDict, taxaDict, nativeBundle, wetlandBundle);
            CreateSitesCsv(csv, siteDict);

			csv.Flush();
			csv.Dispose();
			vegFile.Close();
			return vegFile.FileId;
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

		private static void CreateVegSurveyCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict, Dictionary<CompoundIdentity, Tuple<int, VegSurvey>> surveyDict,
			Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict, Dictionary<CompoundIdentity, Tuple<int, VegPlotType>> plotDict)
		{
			string[] cols = new string[] { "VegSurveyKey", "SamplingEventKey", "SiteKey", "PlotType", "AdHocLocation",
			"Area", "ElevationMin", "ElevationMax", "Description", "IsPrivate" };

			ITable surveys = db.Create("VegSurveys", cols);

			var orderedSurveys = surveyDict.OrderBy(x => x.Value.Item1);
			foreach (var surv in orderedSurveys)
			{
				VegSurvey survey = surv.Value.Item2;
				IRow r = surveys.CreateRow();
				r[0] = surv.Value.Item1.ToString();

                r[1] = eventDict[survey.SampleEventId].Item1.ToString();

                string siteFK = na;
                if (siteDict.ContainsKey(survey.SiteId))
                {
                    siteFK = siteDict[survey.SiteId].Item1.ToString();
                }
                r[2] = siteFK;

                r[3] = survey.PlotTypeId != null ? plotDict[survey.PlotTypeId].Item2.Name : null;
                r[4] = survey.Location != null ? WktUtils.ToWkt(survey.Location as Point2<double>).ToString() : null;
                r[5] = float.IsNaN(survey.Area) ? null : survey.Area.ToString();

                if (survey.ElevationRange != null)
                {
                    r[6] = float.IsNaN(survey.ElevationRange.Min) ? null : survey.ElevationRange.Min.ToString();
                    r[7] = float.IsNaN(survey.ElevationRange.Max) ? null : survey.ElevationRange.Max.ToString();
                }
                else
                {
                    r[6] = null;
                    r[7] = null;
                }

				r[8] = survey.Description;
				r[9] = survey.IsPrivate.ToString();
				surveys.AddRow(r);
			}
			surveys.Flush();
			return;
		}

		private static void CreateVegPlotTypeCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, VegPlotType>> plotDict)
		{
			string[] cols = new string[] { "VegPlotTypeKey", "Name", "Description" };

			ITable plots = db.Create("VegPlotTypes", cols);

			var orderedPlots = plotDict.OrderBy(x => x.Value.Item1);
			foreach (var plot in orderedPlots)
			{
				VegPlotType vegPlot = plot.Value.Item2;
				IRow r = plots.CreateRow();
				r[0] = plot.Value.Item1.ToString();
				r[1] = vegPlot.Name;
				r[2] = vegPlot.Description;
				plots.AddRow(r);
			}
			plots.Flush();
			return;
		}

		private static void CreateVegSampleCsv(CsvDb db, Dictionary<Guid, Tuple<int, VegSample>> vegsDict, Dictionary<CompoundIdentity, Tuple<int, VegSurvey>> surveyDict,
			Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict)
		{
			string[] cols = new string[] { "VegSampleKey", "VegSurveyKey", "SiteKey", "When",
			"AdHocLocation", "ElevationMin", "ElevationMax" };

			ITable vegs = db.Create("VegSamples", cols);

			var orderedVegs = vegsDict.OrderBy(x => x.Value.Item1);
			foreach (var veg in orderedVegs)
			{
				VegSample samp = veg.Value.Item2;
				IRow r = vegs.CreateRow();
				r[0] = veg.Value.Item1.ToString();
				r[1] = surveyDict[samp.VegSurveyId].Item1.ToString();

				string siteFK = na;
				if (siteDict.ContainsKey(samp.SiteId))
				{
					siteFK = siteDict[samp.SiteId].Item1.ToString();
				}
				r[2] = siteFK;

				r[3] = samp.When.ToString();
				r[4] = WktUtils.ToWkt(samp.Location as Point2<double>).ToString();
				r[5] = float.IsNaN(samp.ElevationRange.Min) ? null : samp.ElevationRange.Min.ToString();
				r[6] = float.IsNaN(samp.ElevationRange.Max) ? null : samp.ElevationRange.Max.ToString();
				vegs.AddRow(r);
			}
			vegs.Flush();
			return;
		}

		private static void CreateHerbSampleCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, VegSurvey>> surveyDict, Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict,
            Dictionary<Guid, Tuple<int, HerbSample>> herbDict, Dictionary<Guid, Tuple<int, VegSample>> vegsDict, Dictionary<CompoundIdentity, TaxaUnit> taxaDict,
            EntityBundle nativeBundle, EntityBundle wetlandBundle)
		{
			string[] cols = new string[] { "VegSurveyKey", "SiteKey", "When", "AdHocLocation", "ElevationMin", "ElevationMax",
               "Taxa", "PercentCover", "Description", "IsNative", "IsWetland" };

			ITable herbs = db.Create("HerbSamples", cols);

			var orderedHerbs = herbDict.OrderBy(x => x.Value.Item1);
			foreach (var herb in orderedHerbs)
			{
				HerbSample herbSample = herb.Value.Item2;
				IRow r = herbs.CreateRow();

                //vegSample Info
                VegSample samp = vegsDict[herbSample.VegSampleId].Item2;
                r[0] = (surveyDict[samp.VegSurveyId].Item1.ToString());

                string siteFK = na;  //default
                if (samp.SiteId.IsEmpty)
                {
                    //sample has no site ID defined (adhoc)
                    siteFK = null;
                }
                else if (siteDict.ContainsKey(samp.SiteId))
                {
                    //lookup
                    siteFK = siteDict[samp.SiteId].Item1.ToString();
                }
                r[1] = siteFK;

                r[2] = samp.When.ToString();
                r[3] = samp.Location != null ? WktUtils.ToWkt(samp.Location as Point2<double>).ToString() : null;

                if (samp.ElevationRange != null)
                {
                    r[4] = float.IsNaN(samp.ElevationRange.Min) ? null : samp.ElevationRange.Min.ToString();
                    r[5] = float.IsNaN(samp.ElevationRange.Max) ? null : samp.ElevationRange.Max.ToString();
                }
                else
                {
                    r[4] = null;
                    r[5] = null;
                }
                
				r[6] = taxaDict[herbSample.TaxaUnitId].Name;
				r[7] = float.IsNaN(herbSample.PercentCover) ? null : herbSample.PercentCover.ToString();
				r[8] = herbSample.Description;
               
                BundleElement nativeHerb = nativeBundle.Get(herbSample.TaxaUnitId);
                if (nativeHerb != null)
                    r[9] = "TRUE";
                else
                    r[9] = null;

                BundleElement wetlandHerb = wetlandBundle.Get(herbSample.TaxaUnitId);
                if (wetlandHerb != null)
                    r[10] = wetlandHerb.DisplayName;
                else
                    r[10] = null;

                herbs.AddRow(r);
			}
			herbs.Flush();
			return;
		}

		private static void CreateShrubSampleCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, VegSurvey>> surveyDict, Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict, 
            Dictionary<Guid, Tuple<int, ShrubSample>> shrubDict, Dictionary<Guid, Tuple<int, VegSample>> vegsDict, Dictionary<CompoundIdentity, TaxaUnit> taxaDict,
            EntityBundle nativeBundle, EntityBundle wetlandBundle)
		{
			string[] cols = new string[] { "VegSurveyKey", "SiteKey", "When", "AdHocLocation", "ElevationMin", "ElevationMax",
                "Taxa", "SizeClass", "Count", "Description","IsNative", "IsWetland" };

			ITable shrubs = db.Create("ShrubSamples", cols);

			var orderedShrubs = shrubDict.OrderBy(x => x.Value.Item1);
			foreach (var shrub in orderedShrubs)
			{
				ShrubSample shrubSample = shrub.Value.Item2;
				IRow r = shrubs.CreateRow();

                //vegSample Info
                VegSample samp = vegsDict[shrubSample.VegSampleId].Item2;
                r[0] = (surveyDict[samp.VegSurveyId].Item1.ToString());

                string siteFK = na;  //default
                if (samp.SiteId.IsEmpty)
                {
                    //sample has no site ID defined (adhoc)
                    siteFK = null;
                }
                else if (siteDict.ContainsKey(samp.SiteId))
                {
                    //lookup
                    siteFK = siteDict[samp.SiteId].Item1.ToString();
                }
                r[1] = siteFK;

                r[2] = samp.When.ToString();
                r[3] = samp.Location != null ? WktUtils.ToWkt(samp.Location as Point2<double>).ToString() : null;

                if (samp.ElevationRange != null)
                {
                    r[4] = float.IsNaN(samp.ElevationRange.Min) ? null : samp.ElevationRange.Min.ToString();
                    r[5] = float.IsNaN(samp.ElevationRange.Max) ? null : samp.ElevationRange.Max.ToString();
                }
                else
                {
                    r[4] = null;
                    r[5] = null;
                }

                r[6] = taxaDict[shrubSample.TaxaUnitId].Name;
                r[7] = shrubSample.SizeClass;
                r[8] = shrubSample.Count.ToString();
                r[9] = shrubSample.Description;

                BundleElement nativeHerb = nativeBundle.Get(shrubSample.TaxaUnitId);
                if (nativeHerb != null)
                    r[10] = "TRUE";
                else
                    r[10] = null;

                BundleElement wetlandHerb = wetlandBundle.Get(shrubSample.TaxaUnitId);
                if (wetlandHerb != null)
                    r[11] = wetlandHerb.DisplayName;
                else
                    r[11] = null;

				shrubs.AddRow(r);
			}
			shrubs.Flush();
			return;
		}

		private static void CreateTreeSampleCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, VegSurvey>> surveyDict, Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict, 
            Dictionary<Guid, Tuple<int, TreeSample>> treeDict, Dictionary<Guid, Tuple<int, VegSample>> vegsDict, Dictionary<CompoundIdentity, TaxaUnit> taxaDict,
            EntityBundle nativeBundle, EntityBundle wetlandBundle)
		{
			string[] cols = new string[] { "VegSurveyKey", "SiteKey", "When", "AdHocLocation", "ElevationMin", "ElevationMax",
                "Taxa", "Dbh", "Description", "IsNative", "IsWetland" };

			ITable trees = db.Create("TreeSamples", cols);

			var orderedTrees = treeDict.OrderBy(x => x.Value.Item1);
			foreach (var tree in orderedTrees)
			{
				TreeSample treeSample = tree.Value.Item2;
				IRow r = trees.CreateRow();

                //vegSample Info
                VegSample samp = vegsDict[treeSample.VegSampleId].Item2;
                r[0] = (surveyDict[samp.VegSurveyId].Item1.ToString());

                string siteFK = na;  //default
                if (samp.SiteId.IsEmpty)
                {
                    //sample has no site ID defined (adhoc)
                    siteFK = null;
                }
                else if (siteDict.ContainsKey(samp.SiteId))
                {
                    //lookup
                    siteFK = siteDict[samp.SiteId].Item1.ToString();
                }
                r[1] = siteFK;

                r[2] = samp.When.ToString();
                r[3] = samp.Location != null ? WktUtils.ToWkt(samp.Location as Point2<double>).ToString() : null;

                if (samp.ElevationRange != null)
                {
                    r[4] = float.IsNaN(samp.ElevationRange.Min) ? null : samp.ElevationRange.Min.ToString();
                    r[5] = float.IsNaN(samp.ElevationRange.Max) ? null : samp.ElevationRange.Max.ToString();
                }
                else
                {
                    r[4] = null;
                    r[5] = null;
                }

                r[6] = taxaDict[treeSample.TaxaUnitId].Name;
                r[7] = float.IsNaN(treeSample.DiameterBreastHigh) ? null : treeSample.DiameterBreastHigh.ToString();
                r[8] = treeSample.Description;

                BundleElement nativeHerb = nativeBundle.Get(treeSample.TaxaUnitId);
                if (nativeHerb != null)
                    r[9] = "TRUE";
                else
                    r[9] = null;

                BundleElement wetlandHerb = wetlandBundle.Get(treeSample.TaxaUnitId);
                if (wetlandHerb != null)
                    r[10] = wetlandHerb.DisplayName;
                else
                    r[10] = null;

				trees.AddRow(r);
			}
			trees.Flush();
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

		private Guid CreateExcelFile(Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict, Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict,
			Dictionary<CompoundIdentity, Organization> orgDict, Dictionary<CompoundIdentity, FieldTrip> fieldTripDict, Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict,
			Dictionary<CompoundIdentity, Project> projectDict, Dictionary<CompoundIdentity, Tuple<int, VegSurvey>> surveyDict, Dictionary<CompoundIdentity, Tuple<int, VegPlotType>> plotDict,
			Dictionary<Guid, Tuple<int, VegSample>> vegsDict, Dictionary<Guid, Tuple<int, HerbSample>> herbDict, Dictionary<Guid, Tuple<int, ShrubSample>> shrubDict,
			Dictionary<Guid, Tuple<int, TreeSample>> treeDict, Dictionary<CompoundIdentity, TaxaUnit> taxaDict, EntityBundle nativeBundle, EntityBundle wetlandBundle)
		{
			IFileStoreProvider provider = FileStoreManager.Instance.GetProvider();
			FilestoreFile vegFile = provider.MakeTemp(DateTime.UtcNow.AddHours(4));
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

			XlSchema surveySchema = GetVegSurveySchema();
			XlWorksheet surveySheet = sheets.AddWorksheet("VegSurveys", XlColor.White, surveySchema);
			XlRows surveyRows = surveySheet.Rows;

			var orderedSurveys = surveyDict.OrderBy(x => x.Value.Item1);
			foreach (var surv in orderedSurveys)
			{
				VegSurvey survey = surv.Value.Item2;
				List<string> survItems = new List<string>();

                survItems.Add(surv.Value.Item1.ToString());
                survItems.Add(eventDict[survey.SampleEventId].Item1.ToString());

                string siteFK = na;
				if (siteDict.ContainsKey(survey.SiteId))
				{
					siteFK = siteDict[survey.SiteId].Item1.ToString();
				}
				survItems.Add(siteFK);

				survItems.Add(survey.PlotTypeId != null ? plotDict[survey.PlotTypeId].Item2.Name : null);
				survItems.Add(survey.Location != null ? WktUtils.ToWkt(survey.Location as Point2<double>).ToString() : null);
				survItems.Add(float.IsNaN(survey.Area) ? null : survey.Area.ToString());

                if(survey.ElevationRange != null)
                {
                    survItems.Add(float.IsNaN(survey.ElevationRange.Min) ? null : survey.ElevationRange.Min.ToString());
                    survItems.Add(float.IsNaN(survey.ElevationRange.Max) ? null : survey.ElevationRange.Max.ToString());
                }
                else
                {
                    survItems.Add(null);
                    survItems.Add(null);
                }

				survItems.Add(survey.Description);
				survItems.Add(survey.IsPrivate.ToString());

				SchemaRowData row = new SchemaRowData(surveySchema, survItems);
				surveyRows.AddRow(row);
			}

            XlSchema herbSchema = GetHerbSampleSchema();
			XlWorksheet herbSheet = sheets.AddWorksheet("HerbSamples", XlColor.White, herbSchema);
			XlRows herbRows = herbSheet.Rows;

			var orderedHerbs = herbDict.OrderBy(x => x.Value.Item1);
			foreach (var herb in orderedHerbs)
			{
				HerbSample herbSample = herb.Value.Item2;
				List<string> herbItems = new List<string>();

                //vegSample Info
                VegSample samp = vegsDict[herbSample.VegSampleId].Item2;
                herbItems.Add(surveyDict[samp.VegSurveyId].Item1.ToString());

                string siteFK = na;  //default
                if (samp.SiteId.IsEmpty)
                {
                    //sample has no site ID defined (adhoc)
                    siteFK = null;
                }
                else if (siteDict.ContainsKey(samp.SiteId))
                {
                    //lookup
                    siteFK = siteDict[samp.SiteId].Item1.ToString();
                }
                herbItems.Add(siteFK);

                herbItems.Add(samp.When.ToString());
                herbItems.Add(samp.Location != null ? WktUtils.ToWkt(samp.Location as Point2<double>).ToString() : null);

                if (samp.ElevationRange != null)
                {
                    herbItems.Add(float.IsNaN(samp.ElevationRange.Min) ? null : samp.ElevationRange.Min.ToString());
                    herbItems.Add(float.IsNaN(samp.ElevationRange.Max) ? null : samp.ElevationRange.Max.ToString());
                }
                else
                {
                    herbItems.Add(null);
                    herbItems.Add(null);
                }

                //herbSample Info
                herbItems.Add(taxaDict[herbSample.TaxaUnitId].Name);
				herbItems.Add(float.IsNaN(herbSample.PercentCover) ? null : herbSample.PercentCover.ToString());
				herbItems.Add(herbSample.Description);

                BundleElement nativeHerb = nativeBundle.Get(herbSample.TaxaUnitId);
                if (nativeHerb != null)
                    herbItems.Add("TRUE");
                else
                    herbItems.Add(null);

                BundleElement wetlandHerb = wetlandBundle.Get(herbSample.TaxaUnitId);
                if (wetlandHerb != null)
                    herbItems.Add(wetlandHerb.DisplayName);
                else
                    herbItems.Add(null);

				SchemaRowData row = new SchemaRowData(herbSchema, herbItems);
				herbRows.AddRow(row);
			}

			XlSchema shrubSchema = GetShrubSampleSchema();
			XlWorksheet shrubSheet = sheets.AddWorksheet("ShrubSamples", XlColor.White, shrubSchema);
			XlRows shrubRows = shrubSheet.Rows;

			var orderedShrubs = shrubDict.OrderBy(x => x.Value.Item1);
			foreach (var shrub in orderedShrubs)
			{
				ShrubSample shrubSample = shrub.Value.Item2;
				List<string> shrubItems = new List<string>();

                //vegSample Info
                VegSample samp = vegsDict[shrubSample.VegSampleId].Item2;
                shrubItems.Add(surveyDict[samp.VegSurveyId].Item1.ToString());

                string siteFK = na;  //default
                if (samp.SiteId.IsEmpty)
                {
                    //sample has no site ID defined (adhoc)
                    siteFK = null;
                }
                else if (siteDict.ContainsKey(samp.SiteId))
                {
                    //lookup
                    siteFK = siteDict[samp.SiteId].Item1.ToString();
                }
                shrubItems.Add(siteFK);

                shrubItems.Add(samp.When.ToString());
                shrubItems.Add(samp.Location != null ? WktUtils.ToWkt(samp.Location as Point2<double>).ToString() : null);

                if (samp.ElevationRange != null)
                {
                    shrubItems.Add(float.IsNaN(samp.ElevationRange.Min) ? null : samp.ElevationRange.Min.ToString());
                    shrubItems.Add(float.IsNaN(samp.ElevationRange.Max) ? null : samp.ElevationRange.Max.ToString());
                }
                else
                {
                    shrubItems.Add(null);
                    shrubItems.Add(null);
                }

                //shrub info                
				shrubItems.Add(taxaDict[shrubSample.TaxaUnitId].Name);
				shrubItems.Add(shrubSample.SizeClass);
				shrubItems.Add(shrubSample.Count.ToString());
				shrubItems.Add(shrubSample.Description);

                BundleElement nativeHerb = nativeBundle.Get(shrubSample.TaxaUnitId);
                if (nativeHerb != null)
                    shrubItems.Add("TRUE");
                else
                    shrubItems.Add(null);

                BundleElement wetlandHerb = wetlandBundle.Get(shrubSample.TaxaUnitId);
                if (wetlandHerb != null)
                    shrubItems.Add(wetlandHerb.DisplayName);
                else
                    shrubItems.Add(null);

                SchemaRowData row = new SchemaRowData(shrubSchema, shrubItems);
				shrubRows.AddRow(row);
			}

			XlSchema treeSchema = GetTreeSampleSchema();
			XlWorksheet treeSheet = sheets.AddWorksheet("TreeSamples", XlColor.White, treeSchema);
			XlRows treeRows = treeSheet.Rows;

			var orderedTrees = treeDict.OrderBy(x => x.Value.Item1);
			foreach (var tree in orderedTrees)
			{
				TreeSample treeSample = tree.Value.Item2;
				List<string> treeItems = new List<string>();

                //vegSample Info
                VegSample samp = vegsDict[treeSample.VegSampleId].Item2;
                treeItems.Add(surveyDict[samp.VegSurveyId].Item1.ToString());

                string siteFK = na;  //default
                if (samp.SiteId.IsEmpty)
                {
                    //sample has no site ID defined (adhoc)
                    siteFK = null;
                }
                else if (siteDict.ContainsKey(samp.SiteId))
                {
                    //lookup
                    siteFK = siteDict[samp.SiteId].Item1.ToString();
                }
                treeItems.Add(siteFK);

                treeItems.Add(samp.When.ToString());
                treeItems.Add(samp.Location != null ? WktUtils.ToWkt(samp.Location as Point2<double>).ToString() : null);

                if (samp.ElevationRange != null)
                {
                    treeItems.Add(float.IsNaN(samp.ElevationRange.Min) ? null : samp.ElevationRange.Min.ToString());
                    treeItems.Add(float.IsNaN(samp.ElevationRange.Max) ? null : samp.ElevationRange.Max.ToString());
                }
                else
                {
                    treeItems.Add(null);
                    treeItems.Add(null);
                }

                //tree info
				treeItems.Add(taxaDict[treeSample.TaxaUnitId].Name);
				treeItems.Add(float.IsNaN(treeSample.DiameterBreastHigh) ? null : treeSample.DiameterBreastHigh.ToString());
				treeItems.Add(treeSample.Description);
				
                BundleElement nativeHerb = nativeBundle.Get(treeSample.TaxaUnitId);
                if (nativeHerb != null)
                    treeItems.Add("TRUE");
                else
                    treeItems.Add(null);

                BundleElement wetlandHerb = wetlandBundle.Get(treeSample.TaxaUnitId);
                if (wetlandHerb != null)
                    treeItems.Add(wetlandHerb.DisplayName);
                else
                    treeItems.Add(null);

                SchemaRowData row = new SchemaRowData(treeSchema, treeItems);
				treeRows.AddRow(row);
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

			book.Save(vegFile);
			vegFile.Flush();
			vegFile.Close();
			vegFile.Dispose();
			return vegFile.FileId;
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

		private static XlSchema GetVegSurveySchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("VegSurveyKey", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("SamplingEventKey", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("SiteKey", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("PlotType", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("AdHocLocation", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Area", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("ElevationMin", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("ElevationMax", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsPrivate", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetVegPlotTypeSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("VegPlotTypeKey", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Name", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetVegSampleSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("VegSampleKey", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("VegSurveyKey", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("SiteKey", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("When", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("AdHocLocation", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("ElevationMin", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("ElevationMax", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetHerbSampleSchema()
		{
			XlSchema schema = new XlSchema();
            schema.AddColumn("VegSurveyKey", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("SiteKey", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("When", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("AdHocLocation", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ElevationMin", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ElevationMax", typeof(string), StyleSheetHelper.Normal);           

            schema.AddColumn("Taxa", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("PercentCover", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsNative", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsWetland", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetShrubSampleSchema()
		{
			XlSchema schema = new XlSchema();
            schema.AddColumn("VegSurveyKey", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("SiteKey", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("When", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("AdHocLocation", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ElevationMin", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ElevationMax", typeof(string), StyleSheetHelper.Normal);
            
			schema.AddColumn("Taxa", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("SizeClass", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Count", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsNative", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsWetland", typeof(string), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetTreeSampleSchema()
		{
			XlSchema schema = new XlSchema();
            schema.AddColumn("VegSurveyKey", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("SiteKey", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("When", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("AdHocLocation", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ElevationMin", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("ElevationMax", typeof(string), StyleSheetHelper.Normal);

			schema.AddColumn("Taxa", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Dbh", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsNative", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("IsWetland", typeof(string), StyleSheetHelper.Normal);
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
