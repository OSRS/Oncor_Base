using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Net.Http.Listener;
using Osrs.Net.Http.Routing;
using Osrs.Runtime;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using Osrs.Runtime.Logging.Providers;
using Osrs.Runtime.Services;
using Pnnl.Oncor.Rest.EntityBundles;
using Pnnl.Oncor.Rest.FieldActivities;
using Pnnl.Oncor.Rest.FileTransfer;
using Pnnl.Oncor.Rest.Organizations;
using Pnnl.Oncor.Rest.Projects;
using Pnnl.Oncor.Rest.Security;
using Pnnl.Oncor.Rest.Sites;
using Pnnl.Oncor.Rest.Instruments;
using Pnnl.Oncor.Rest.Persons;
using Pnnl.Oncor.Rest.Dets;
using Pnnl.Oncor.Rest.UserAffiliation;
using System.Collections.Generic;
using Pnnl.Oncor.Rest.Taxa;
using Pnnl.Oncor.Rest.WQ;
using Pnnl.Oncor.Rest.Fish;
using Pnnl.Oncor.Rest.Vegetation;
using Pnnl.Oncor.Rest.UserProfile;

namespace Pnnl.Oncor.Host
{
	internal static class HttpModules
	{
		internal static HttpListenerServer Server;

		internal static bool Initialize()
		{
			LogProviderBase log = null;
			if (LogManager.Instance.State == RunState.Running)
				log = LogManager.Instance.GetProvider(typeof(OncorServer));
			if (log == null)
				log = new NullLogger(typeof(OncorServer));

			ConfigurationProviderBase prov = ConfigurationManager.Instance.GetProvider();
			if (prov != null)
			{
				ConfigurationParameter param = prov.Get(typeof(HttpListenerServerListener), "listenerUrls");
				List<string> listenUrls = new List<string>();
				if (param != null)
				{
					string[] tmp = (string[])param.Value;
					listenUrls.AddRange(tmp);

					List<IHandlerMapper> handlers = new List<IHandlerMapper>();
					handlers.Add(InitApi(prov));
					handlers.Add(InitFiles(prov));
					Server = InitServer(listenUrls, handlers);
					return Server != null;
				}
			}
			return false;
		}

		private static HttpListenerServer InitServer(IEnumerable<string> listenUrls, IEnumerable<IHandlerMapper> handlerMappers)
		{
			ServerTaskPoolOptions options = new ServerTaskPoolOptions();
			ServerRouting router = new ServerRouting();

			foreach (MapHandler cur in handlerMappers)
			{
				if (cur != null)
				{
					router.Map.Add(cur);
				}
			}

			if (router.Map.Count > 0)
			{
				HttpListenerServerListener listener = new HttpListenerServerListener(listenUrls, router);
				HttpListenerServer server = HttpListenerServer.Create(listener, options, false);

				return server;
			}

			return null;
		}

		private static IHandlerMapper InitApi(ConfigurationProviderBase prov)
		{
			string root = "api/";
			VerbRestrictingMatchRule verbs = new VerbRestrictingMatchRule(HttpVerbs.GET.ToString(), HttpVerbs.POST.ToString());
			UrlBaseMatchRule url = new UrlBaseMatchRule(new string[] { root });
			verbs.NextMatcher = url;

			ServerRouting router = new ServerRouting();
			SessionIdHeader head = new SessionIdHeader();
			head.Next = router;
			MatchRuleMapHandler mapper = new MatchRuleMapHandler(head, verbs);

			//add all other handlers
			Session s = new Session();
			router.Map.Add(new UrlBaseMapHandler(s, root + s.BaseUrl));

			Login l = new Login();
			router.Map.Add(new UrlBaseMapHandler(l, root + l.BaseUrl));

			UserAuthorizations au = new UserAuthorizations();
			router.Map.Add(new UrlBaseMapHandler(au, root + au.BaseUrl));

			UserAffiliationHandler uah = new UserAffiliationHandler();
			router.Map.Add(new UrlBaseMapHandler(uah, root + uah.BaseUrl));

			Request req = new Request();
			router.Map.Add(new UrlBaseMapHandler(req, root + req.BaseUrl));

			OrganizationHandler orgs = new OrganizationHandler();
			router.Map.Add(new UrlBaseMapHandler(orgs, root + orgs.BaseUrl));

			SitesHandler sites = new SitesHandler();
			router.Map.Add(new UrlBaseMapHandler(sites, root + sites.BaseUrl));

			ProjectsHandler projs = new ProjectsHandler();
			router.Map.Add(new UrlBaseMapHandler(projs, root + projs.BaseUrl));

			FieldActivitiesHandler fas = new FieldActivitiesHandler();
			router.Map.Add(new UrlBaseMapHandler(fas, root + fas.BaseUrl));

			FileTransferHandler fts = new FileTransferHandler();
			router.Map.Add(new UrlBaseMapHandler(fts, root + fts.BaseUrl));

			InstrumentsHandler inst = new InstrumentsHandler();
			router.Map.Add(new UrlBaseMapHandler(inst, root + inst.BaseUrl));

			PersonsHandler per = new PersonsHandler();
			router.Map.Add(new UrlBaseMapHandler(per, root + per.BaseUrl));

			TaxaHandler tax = new TaxaHandler();
			router.Map.Add(new UrlBaseMapHandler(tax, root + tax.BaseUrl));

			WQHandler wq = new WQHandler();
			router.Map.Add(new UrlBaseMapHandler(wq, root + wq.BaseUrl));

			FishHandler fish = new FishHandler();
			router.Map.Add(new UrlBaseMapHandler(fish, root + fish.BaseUrl));

			VegetationHandler veg = new VegetationHandler();
			router.Map.Add(new UrlBaseMapHandler(veg, root + veg.BaseUrl));

			EntityBundlesHandler ebh = new EntityBundlesHandler();
			router.Map.Add(new UrlBaseMapHandler(ebh, root + ebh.BaseUrl));

			DetsHandler det = new DetsHandler();
			router.Map.Add(new UrlBaseMapHandler(det, root + det.BaseUrl));

			return mapper;
		}

		private static IHandlerMapper InitFiles(ConfigurationProviderBase prov)
		{
			ConfigurationParameter param = prov.Get(typeof(SimpleFileHandler), "rootDirectory");
			if (param != null)
			{
				string rootDir = (string)param.Value;

				param = prov.Get(typeof(SimpleFileHandler), "logicalDirectory");
				if (param != null)
				{
					string localDir = (string)param.Value;

					string[] defFiles;
					param = prov.Get(typeof(SimpleFileHandler), "defaultFiles");
					if (param != null)
					{
						defFiles = (string[])param.Value;

						FileExtensions exts = new FileExtensions();
						param = prov.Get(typeof(SimpleFileHandler), "allowedExtensions");
						if (param != null)
						{
							string[] tmp = (string[])param.Value;
							foreach (string cur in tmp)
							{
								exts.Add(cur);
							}
							SimpleFileHandler handler = new SimpleFileHandler(rootDir, defFiles, MimeTypes.GetAllWellKnown(), exts, new FileExtensions()); //so we can also have static files
							return new UrlBaseMapHandler(handler, new string[] { localDir });
						}
					}
				}
			}

			return null;
		}
	}
}
