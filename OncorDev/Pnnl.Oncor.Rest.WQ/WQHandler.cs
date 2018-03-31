using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Oncor.Excel;
using Osrs.Oncor.FileStore;
using Osrs.Oncor.WellKnown.WaterQuality;
using Osrs.Oncor.WellKnown.WaterQuality.Module;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using Osrs.WellKnown.Organizations;
using Osrs.WellKnown.Projects;
using Osrs.WellKnown.FieldActivities;
using Osrs.WellKnown.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Osrs.Numerics.Spatial.Geometry;
using System.IO;
using Osrs.Oncor.SimpleDb;

namespace Pnnl.Oncor.Rest.WQ
{
	public sealed class WQHandler : HttpHandlerBase, IServiceHandler
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
				return "wq";
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
							if (meth.Equals("deployments", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									HashSet<CompoundIdentity> eventIds = null;
									HashSet<CompoundIdentity> deploymentIds = null;
									HashSet<CompoundIdentity> siteIds = null;
									DateTime? start = null;
									DateTime? end = null;

									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{

										if (token["events"] != null)
											eventIds = JsonUtils.ToIds(token["events"]);

										if (token["deployments"] != null)
											deploymentIds = JsonUtils.ToIds(token["deployments"]);

										if (token["sites"] != null)
											siteIds = JsonUtils.ToIds(token["sites"]);

										if (token["start"] != null)
											start = JsonUtils.ToDate(token["start"]);

										if (token["end"] != null)
											end = JsonUtils.ToDate(token["end"]);
									}

									IWQDeploymentProvider provider = WaterQualityManager.Instance.GetDeploymentProvider(ctx); //filters WQ deployments by user context
									if(provider != null)
									{
										IEnumerable<WaterQualityDeployment> deployments = GetDeployments(provider, eventIds, deploymentIds, siteIds, start, end);
										JArray jdeployments = Jsonifier.ToJson(deployments);

										if (jdeployments != null)
										{
											RestUtils.Push(context.Response, JsonOpStatus.Ok, jdeployments.ToString());
											return;
										}
										else
										{
											RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
											return;
										}
									}
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
							else if (meth.Equals("measurements", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									//TODO: ask about iterative calls for deployments (each deployment is a separate db call)

									IWQMeasurementProvider provider = WaterQualityManager.Instance.GetMeasurementProvider(ctx);
									if (provider != null)
									{
										HashSet<CompoundIdentity> eventIds = null;
										HashSet<CompoundIdentity> deploymentIds = null;
										HashSet<CompoundIdentity> siteIds = null;
										DateTime? start = null;
										DateTime? end = null;

										JToken token = JsonUtils.GetDataPayload(context.Request);
										if (token != null)
										{
											if (token["events"] != null)
												eventIds = JsonUtils.ToIds(token["events"]);

											if (token["deployments"] != null)
												deploymentIds = JsonUtils.ToIds(token["deployments"]);

											if (token["sites"] != null)
												siteIds = JsonUtils.ToIds(token["sites"]);

											if (token["start"] != null)
												start = JsonUtils.ToDate(token["start"]);

											if (token["end"] != null)
												end = JsonUtils.ToDate(token["end"]);
										}

										IWQDeploymentProvider depProvider = WaterQualityManager.Instance.GetDeploymentProvider(ctx); //provider will autofilter WQ deployments by user context
										IEnumerable<WaterQualityDeployment> deployments = GetDeployments(depProvider, eventIds, deploymentIds, siteIds, null, null);
									   
										List<WaterQualityMeasurement> measurements = new List<WaterQualityMeasurement>();

										if (start != null || end != null)
										{
											DateTime queryStart;
											DateTime queryEnd;
											if (start == null)
												queryStart = WQUtils.GlobalMinDate;
											else
												queryStart = start.Value;

											if (end == null)
												queryEnd = DateTime.UtcNow;
											else
												queryEnd = end.Value;

											foreach (WaterQualityDeployment dep in deployments)
												measurements.AddRange(provider.Get(dep.Identity, queryStart, queryEnd));

										}
										else
										{
											foreach (WaterQualityDeployment dep in deployments)
												measurements.AddRange(provider.Get(dep.Identity));
										}

										JArray jmeasurements = Jsonifier.ToJson(measurements);

										if (jmeasurements != null)
											RestUtils.Push(context.Response, JsonOpStatus.Ok, jmeasurements.ToString());
										else
											RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
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
							else if (meth.Equals("export", StringComparison.OrdinalIgnoreCase))
							{
								try
								{									
									HashSet<CompoundIdentity> eventIds = null;
									HashSet<CompoundIdentity> deploymentIds = null;
									HashSet<CompoundIdentity> siteIds = null;
									DateTime? start = null;
									DateTime? end = null;

									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["events"] != null)
											eventIds = JsonUtils.ToIds(token["events"]);

										if (token["deployments"] != null)
											deploymentIds = JsonUtils.ToIds(token["deployments"]);

										if (token["sites"] != null)
											siteIds = JsonUtils.ToIds(token["sites"]);

										if (token["start"] != null)
											start = JsonUtils.ToDate(token["start"]);

										if (token["end"] != null)
											end = JsonUtils.ToDate(token["end"]);
									}

									IWQDeploymentProvider depProvider = WaterQualityManager.Instance.GetDeploymentProvider(ctx);
									IWQMeasurementProvider measProvider = WaterQualityManager.Instance.GetMeasurementProvider(ctx);
									ISiteProvider siteProvider = SiteManager.Instance.GetSiteProvider(ctx);
									ISampleEventProvider sampProvider = FieldActivityManager.Instance.GetSampleEventProvider(ctx);
									IOrganizationProvider orgProvider = OrganizationManager.Instance.GetOrganizationProvider(ctx);
									IFieldTripProvider fieldTripProvider = FieldActivityManager.Instance.GetFieldTripProvider(ctx);
									IFieldActivityProvider fieldActivityProvider = FieldActivityManager.Instance.GetFieldActivityProvider(ctx);
									IProjectProvider projectProvider = ProjectManager.Instance.GetProvider(ctx);

									if (depProvider != null && measProvider != null && siteProvider != null && sampProvider != null &&
										orgProvider != null && fieldTripProvider != null && fieldActivityProvider != null && projectProvider != null)
									{
										IEnumerable<WaterQualityDeployment> deployments = GetDeployments(depProvider, eventIds, deploymentIds, siteIds, null, null);  //on export, time filters apply to measurements only
										IEnumerable<WaterQualityMeasurement> measurements = GetMeasurements(measProvider, start, end, deployments);

										//Get sites and sample events
										List<CompoundIdentity> selected_siteIds = deployments.Select(x => x.SiteId).ToList();
										List<CompoundIdentity> selected_eventIds = deployments.Select(x => x.SampleEventId).ToList();
										IEnumerable<Site> sitesData = GetSites(siteProvider, selected_siteIds);
										IEnumerable<SamplingEvent> eventsData = sampProvider.Get(selected_eventIds);

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

										Guid fileId = CreateDeploymentFile(eventsData, deployments, measurements, sitesData,
											orgData, fieldTripData, fieldActivityData, projectData);

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

		private IEnumerable<WaterQualityDeployment> GetDeployments(IWQDeploymentProvider provider, HashSet<CompoundIdentity> eventIds, HashSet<CompoundIdentity> deploymentIds, HashSet<CompoundIdentity> siteIds, DateTime? start, DateTime? end)
		{
			IEnumerable<WaterQualityDeployment> deployments = provider.Get();
			if (deploymentIds != null && deploymentIds.Count > 0)
			{
				deployments = deployments.Where(x => deploymentIds.Contains(x.Identity));
			}
			if (eventIds != null && eventIds.Count > 0)
			{
				deployments = deployments.Where(x => eventIds.Contains(x.SampleEventId));
			}
			if (siteIds != null && siteIds.Count > 0)
			{
				deployments = deployments.Where(x => siteIds.Contains(x.SiteId));
			}
			if (start != null)
			{
				//TODO: inclusive date or any overlap?
				deployments = deployments.Where(x => x.Range.EndDate >= start);
			}
			if (end != null)
			{
				deployments = deployments.Where(x => x.Range.StartDate <= end);
			}
			return deployments;
		}

		private List<WaterQualityMeasurement> GetMeasurements(IWQMeasurementProvider provider, DateTime? start, DateTime? end, IEnumerable<WaterQualityDeployment> deployments)
		{
			List<WaterQualityMeasurement> measurements = new List<WaterQualityMeasurement>();
			if (start != null || end != null)
			{
				DateTime queryStart;
				DateTime queryEnd;
				if (start == null)
					queryStart = WQUtils.GlobalMinDate;
				else
					queryStart = start.Value;

				if (end == null)
					queryEnd = DateTime.UtcNow;
				else
					queryEnd = end.Value;

				foreach (WaterQualityDeployment dep in deployments)
					measurements.AddRange(provider.Get(dep.Identity, queryStart, queryEnd));

			}
			else
			{
				foreach (WaterQualityDeployment dep in deployments)
					measurements.AddRange(provider.Get(dep.Identity));
			}
			return measurements;
		}

		private List<Site> GetSites(ISiteProvider provider, IEnumerable<CompoundIdentity> siteIds)
		{
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

		private Guid CreateDeploymentFile(IEnumerable<SamplingEvent> events, IEnumerable<WaterQualityDeployment> deployments,
			IEnumerable<WaterQualityMeasurement> measurements, IEnumerable<Site> sites, IEnumerable<Organization> orgs,
			IEnumerable<FieldTrip> fieldTrips, IEnumerable<FieldActivity> fieldActivities, IEnumerable<Project> projects)
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

			i = 1;
			Dictionary<CompoundIdentity, Tuple<int, WaterQualityDeployment>> deploymentDict = new Dictionary<CompoundIdentity, Tuple<int, WaterQualityDeployment>>();
			foreach (WaterQualityDeployment deploy in deployments)
			{
				if (!deploymentDict.ContainsKey(deploy.Identity))
				{
					deploymentDict.Add(deploy.Identity, new Tuple<int, WaterQualityDeployment>(i, deploy));
					i++;
				}
			}

			i = 1;
			List<Tuple<int, WaterQualityMeasurement>> measurementList = new List<Tuple<int, WaterQualityMeasurement>>();
			foreach (WaterQualityMeasurement meas in measurements)
			{
				measurementList.Add(new Tuple<int, WaterQualityMeasurement>(i, meas));
				i++;
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
			foreach (FieldTrip ft in fieldTrips)
			{
				if (!fieldTripDict.ContainsKey(ft.Identity))
				{
					fieldTripDict.Add(ft.Identity, ft);
				}
			}

			Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict = new Dictionary<CompoundIdentity, FieldActivity>();
			foreach (FieldActivity fa in fieldActivities)
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

			int entryCount = siteDict.Count + eventDict.Count + deploymentDict.Count + measurementList.Count + 
				orgDict.Count + fieldTripDict.Count + fieldActivityDict.Count + projectDict.Count;

			if (entryCount <= excelDataCutoff)
			{
				fileExtension = "xlsx";
				return CreateExcelFile(siteDict, eventDict, deploymentDict, measurementList, orgDict, fieldTripDict, fieldActivityDict, projectDict);
			}
			else
			{
				fileExtension = "zip";
				return CreateCsvFile(siteDict, eventDict, deploymentDict, measurementList, orgDict, fieldTripDict, fieldActivityDict, projectDict);
			}
		}

		private Guid CreateCsvFile(Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict, Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict,
			Dictionary<CompoundIdentity, Tuple<int, WaterQualityDeployment>> deploymentDict, List<Tuple<int, WaterQualityMeasurement>> measurementList,
			Dictionary<CompoundIdentity, Organization> orgDict, Dictionary<CompoundIdentity, FieldTrip> fieldTripDict,
			Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict, Dictionary<CompoundIdentity, Project> projectDict)
		{
			IFileStoreProvider provider = FileStoreManager.Instance.GetProvider();

			//Setting up temp file
			FilestoreFile deployFile = provider.MakeTemp(DateTime.UtcNow.AddHours(4));
			CsvDb csv = CsvDb.Create(deployFile);

			CreateSampleEventCsv(csv, eventDict, orgDict, fieldTripDict, fieldActivityDict, projectDict);
			CreateMeasurementsCsv(csv, siteDict, eventDict, deploymentDict, measurementList);
			CreateSitesCsv(csv, siteDict);

			csv.Flush();
			csv.Dispose();
			deployFile.Close();
			return deployFile.FileId;
		}

		private static void CreateSampleEventCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict,
			Dictionary<CompoundIdentity, Organization> orgDict, Dictionary<CompoundIdentity, FieldTrip> fieldTripDict,
			Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict, Dictionary<CompoundIdentity, Project> projectDict)
		{
			//string[] cols = new string[] { "Org", "FieldTrip", "Activity", "Project", "SampleEvent", "SampleEventKey","SampleEventDesc", "SampleEventStart", "SampleEventEnd" };
			string[] cols = new string[] { "Org", "FieldTrip", "Activity", "Project", "SampleEvent", "SampleEventKey", "SampleEventDesc" };

			ITable samp = db.Create("SamplingEvents" ,cols);

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

				IRow r = samp.CreateRow();
				r[0] = orgName;
				r[1] = ftripName;
				r[2] = factivityName;
				r[3] = projName;
				r[4] = evt.Value.Item2.Name;
				r[5] = evt.Value.Item1.ToString();
				r[6] = evt.Value.Item2.Description;
				//r[7] = evt.Value.Item2.DateRange.Min.ToString();
				//r[8] = evt.Value.Item2.DateRange.Max.ToString();
				samp.AddRow(r);
			}
			samp.Flush();
			return;
		}

		private static void CreateMeasurementsCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict, Dictionary<CompoundIdentity, Tuple<int, SamplingEvent>> eventDict,
			Dictionary<CompoundIdentity, Tuple<int, WaterQualityDeployment>> deploymentDict, List<Tuple<int, WaterQualityMeasurement>> measurementList)
		{
			string[] cols = new string[] { "DataKey", "Deployment", "DeployDesc", "SamplingEventKey", "SiteKey", "DeployStart",
			"DeployEnd", "SampleDate", "SurfaceElevation", "Temperature", "pH", "DissolvedOxygen", "Conductivity", "Salinity", "Velocity" };

			ITable measurements = db.Create("WaterQualityMeasurements", cols);

			var orderedMeasurements = measurementList.OrderBy(x => x.Item1);
			foreach (var meas in orderedMeasurements)
			{
				WaterQualityDeployment deploy = deploymentDict[meas.Item2.DeploymentId].Item2;
				WaterQualityMeasurement measurement = meas.Item2;
				int eventIndex = eventDict[deploy.SampleEventId].Item1;

				//deploy.SiteId could be a dangling reference
				string siteFK = na;
				if (siteDict.ContainsKey(deploy.SiteId))
				{
					siteFK = siteDict[deploy.SiteId].Item1.ToString();
				}

				IRow r = measurements.CreateRow();
				r[0] = meas.Item1.ToString();
				r[1] = deploy.Name;
				r[2] = deploy.Description;
				r[3] = eventIndex.ToString();
				r[4] = siteFK;
				r[5] = deploy.Range.StartDate.ToString();
				r[6] = deploy.Range.EndDate.ToString();
				r[7] = measurement.SampleDate.ToString();
				r[8] = measurement.SurfaceElevation.ToString();
				r[9] = measurement.Temperature.ToString();
				r[10] = measurement.pH.ToString();
				r[11] = measurement.DissolvedOxygen.ToString();
				r[12] = measurement.Conductivity.ToString();
				r[13] = measurement.Salinity.ToString();
				r[14] = measurement.Velocity.ToString();
				measurements.AddRow(r);
			}
			measurements.Flush();
			return;
		}

		private static void CreateSitesCsv(CsvDb db, Dictionary<CompoundIdentity, Tuple<int, Site>> siteDict)
		{
			string[] cols = new string[] { "SiteKey", "Name", "Description", "Location", "LocationMark"};

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
			Dictionary<CompoundIdentity, Tuple<int, WaterQualityDeployment>> deploymentDict, List<Tuple<int, WaterQualityMeasurement>> measurementList,
			Dictionary<CompoundIdentity, Organization> orgDict, Dictionary<CompoundIdentity, FieldTrip> fieldTripDict, 
			Dictionary<CompoundIdentity, FieldActivity> fieldActivityDict, Dictionary<CompoundIdentity, Project> projectDict)
		{
			IFileStoreProvider provider = FileStoreManager.Instance.GetProvider();

			//Setting up file and Excel Workbook
			FilestoreFile deployFile = provider.MakeTemp(DateTime.UtcNow.AddHours(4));
			XlWorkbook book = new XlWorkbook();
			XlWorksheets sheets = book.Worksheets;

			//Generating Sampling Event Sheet
			XlSchema eventSchema = GetSampleEventSchema();
			XlWorksheet eventSheet = sheets.AddWorksheet("SamplingEvents", XlColor.White, eventSchema);
			XlRows eventRows = eventSheet.Rows;

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
				evtItems.Add(projName);
				evtItems.Add(factivityName);
				evtItems.Add(ftripName);
				evtItems.Add(evt.Value.Item2.Name);
				evtItems.Add(evt.Value.Item1.ToString());
				evtItems.Add(evt.Value.Item2.Description);
				//evtItems.Add(evt.Value.Item2.DateRange.Min.ToString());
				//evtItems.Add(evt.Value.Item2.DateRange.Max.ToString());
				
				SchemaRowData row = new SchemaRowData(eventSchema, evtItems);
				eventRows.AddRow(row);
			}

			//Generating Deployment/Measurement Sheet
			XlSchema measSchema = GetDeployMeasurementSchema();
			XlWorksheet measSheet = sheets.AddWorksheet("WaterQualityMeasurements", XlColor.White, measSchema);
			XlRows measRows = measSheet.Rows;

			var orderedMeasurements = measurementList.OrderBy(x => x.Item1);
			foreach (var meas in orderedMeasurements)
			{
				WaterQualityDeployment deploy = deploymentDict[meas.Item2.DeploymentId].Item2;
				WaterQualityMeasurement measurement = meas.Item2;
				int eventIndex = eventDict[deploy.SampleEventId].Item1;

				//deploy.SiteId could be a dangling reference
				string siteFK = na;
				if (siteDict.ContainsKey(deploy.SiteId))
				{
					siteFK = siteDict[deploy.SiteId].Item1.ToString();
				}

				List<string> measItems = new List<string>();
				measItems.Add(meas.Item1.ToString());
				measItems.Add(deploy.Name);
				measItems.Add(deploy.Description);
				measItems.Add(eventIndex.ToString());
				measItems.Add(siteFK);
				measItems.Add(deploy.Range.StartDate.ToString());
				measItems.Add(deploy.Range.EndDate.ToString());
				measItems.Add(measurement.SampleDate.ToString());
				measItems.Add(measurement.SurfaceElevation.ToString());
				measItems.Add(measurement.Temperature.ToString());
				measItems.Add(measurement.pH.ToString());
				measItems.Add(measurement.DissolvedOxygen.ToString());
				measItems.Add(measurement.Conductivity.ToString());
				measItems.Add(measurement.Salinity.ToString());
				measItems.Add(measurement.Velocity.ToString());
				SchemaRowData row = new SchemaRowData(measSchema, measItems);
				measRows.AddRow(row);
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

			book.Save(deployFile);
			deployFile.Flush();
			deployFile.Close();
			deployFile.Dispose();

			return deployFile.FileId;
		}

		private static XlSchema GetSampleEventSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("Org", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("FieldTrip", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Activity", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("Project", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("SampleEvent", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("SampleEventKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("SampleEventDesc", typeof(string), StyleSheetHelper.Normal);
			//schema.AddColumn("SampleEventStart", typeof(DateTime), StyleSheetHelper.Normal);
			//schema.AddColumn("SampleEventEnd", typeof(DateTime), StyleSheetHelper.Normal);
			return schema;
		}

		private static XlSchema GetDeployMeasurementSchema()
		{
			XlSchema schema = new XlSchema();
			schema.AddColumn("DataKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("Deployment", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("DeployDesc", typeof(string), StyleSheetHelper.Normal);
			schema.AddColumn("SamplingEventKey", typeof(int), StyleSheetHelper.Normal);
			schema.AddColumn("SiteKey", typeof(string), StyleSheetHelper.Normal);  //must be a string in case of "unknown or unauthorized", to avoid excel xml validation issues
			schema.AddColumn("DeployStart", typeof(DateTime), StyleSheetHelper.Normal);
			schema.AddColumn("DeployEnd", typeof(DateTime), StyleSheetHelper.Normal);
			schema.AddColumn("SampleDate", typeof(DateTime), StyleSheetHelper.Normal);
			schema.AddColumn("SurfaceElevation", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("Temperature", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("pH", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("DissolvedOxygen", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("Conductivity", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("Salinity", typeof(double), StyleSheetHelper.Normal);
			schema.AddColumn("Velocity", typeof(double), StyleSheetHelper.Normal);
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
