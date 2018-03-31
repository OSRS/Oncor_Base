using Osrs.Runtime;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using Osrs.Security.Authentication;
using Osrs.Security.Authorization;
using Osrs.Security.Identity;
using Osrs.Security.Sessions;
using Osrs.WellKnown.Organizations;
using Osrs.WellKnown.OrganizationHierarchies;
using Osrs.WellKnown.Projects;
using Osrs.WellKnown.Sites;
using Osrs.WellKnown.SensorsAndInstruments;
using Osrs.WellKnown.UserAffiliation;
using Osrs.Oncor.UserAffiliationPermissionChecks;
using Osrs.WellKnown.FieldActivities;
using Osrs.Oncor.Wellknown.Persons;
using Osrs.Oncor.EntityBundles;
using Osrs.Oncor.FileStore;
using Osrs.WellKnown.Taxonomy;
using Osrs.Oncor.WellKnown.WaterQuality.Module;
using Pnnl.Oncor.DetProcessor;
using Osrs.Oncor.WellKnown.Fish.Module;
using Osrs.Oncor.WellKnown.Vegetation.Module;
using System;

namespace Pnnl.Oncor.Host
{
    internal static class RuntimeModules
    {
        internal static bool Application()
        {
            EntityBundleManager.Instance.Initialize();
            EntityBundleManager.Instance.Start();
            if (EntityBundleManager.Instance.State != RunState.Running)
                return false;

            FileStoreManager.Instance.Initialize();
            FileStoreManager.Instance.Start();
            if (FileStoreManager.Instance.State != RunState.Running)
                return false;

            OrganizationManager.Instance.Initialize();
            OrganizationManager.Instance.Start();
            if (OrganizationManager.Instance.State != RunState.Running)
                return false;

            OrganizationHierarchyManager.Instance.Initialize();
            OrganizationHierarchyManager.Instance.Start();
            if (OrganizationHierarchyManager.Instance.State != RunState.Running)
                return false;

            SiteManager.Instance.Initialize();
            SiteManager.Instance.Start();
            if (SiteManager.Instance.State != RunState.Running)
                return false;

            ProjectManager.Instance.Initialize();
            ProjectManager.Instance.Start();
            if (ProjectManager.Instance.State != RunState.Running)
                return false;

            FieldActivityManager.Instance.Initialize();
            FieldActivityManager.Instance.Start();
            if (FieldActivityManager.Instance.State != RunState.Running)
                return false;

            PersonManager.Instance.Initialize();
            PersonManager.Instance.Start();
            if (PersonManager.Instance.State != RunState.Running)
                return false;

            InstrumentManager.Instance.Initialize();
            InstrumentManager.Instance.Start();
            if (InstrumentManager.Instance.State != RunState.Running)
                return false;

            TaxonomyManager.Instance.Initialize();
            TaxonomyManager.Instance.Start();
            if (TaxonomyManager.Instance.State != RunState.Running)
                return false;

            WaterQualityManager.Instance.Initialize();
            WaterQualityManager.Instance.Start();
            if (WaterQualityManager.Instance.State != RunState.Running)
                return false;

            FishManager.Instance.Initialize();
            FishManager.Instance.Start();
            if (FishManager.Instance.State != RunState.Running)
                return false;

            VegetationManager.Instance.Initialize();
            VegetationManager.Instance.Start();
            if (VegetationManager.Instance.State != RunState.Running)
                return false;

            UserAffiliationManager.Instance.Initialize();
            UserAffiliationManager.Instance.Start();
            if (UserAffiliationManager.Instance.State != RunState.Running)
                return false;

            UserAffilationSecurityManager.Instance.Initialize();
            UserAffilationSecurityManager.Instance.Start();
            if (UserAffilationSecurityManager.Instance.State != RunState.Running)
                return false;

            DetProcessorManager.Instance.Initialize();
            DetProcessorManager.Instance.Start();
            if (DetProcessorManager.Instance.State != RunState.Running)
                return false;

            return true;
        }

        internal static bool InitConfigAndLog()
        {
            //Do checks since when running under AppHost,that will initialize config/logging for us
            if (ConfigurationManager.Instance.State != RunState.Running)
            {
                if (RuntimeUtils.Bootstrappable(ConfigurationManager.Instance.State))
                    ConfigurationManager.Instance.Bootstrap();
                if (RuntimeUtils.Initializable(ConfigurationManager.Instance.State))
                    ConfigurationManager.Instance.Initialize();
                if (RuntimeUtils.Startable(ConfigurationManager.Instance.State))
                    ConfigurationManager.Instance.Start();
            }
            if (ConfigurationManager.Instance.State == RunState.Running)
            {
                if (LogManager.Instance.State != RunState.Running)
                {
                    if (RuntimeUtils.Bootstrappable(LogManager.Instance.State))
                        LogManager.Instance.Bootstrap();
                    if (RuntimeUtils.Initializable(LogManager.Instance.State))
                        LogManager.Instance.Initialize();
                    if (RuntimeUtils.Startable(LogManager.Instance.State))
                        LogManager.Instance.Start();
                }
                if (LogManager.Instance.State == RunState.Running)
                {
                    logger = LogManager.Instance.GetProvider(typeof(OncorServer));
                    return true;
                }
            }
            return false;
        }

        internal static bool InitSecurity()
        {
            if (ConfigurationManager.Instance.State == RunState.Running)
            {
                if (LogManager.Instance.State == RunState.Running)
                {
                    string method = "InitSecurity";
                    try
                    {
                        Log(method, LogLevel.Info, "Starting AuthorizationManager");
                        AuthorizationManager.Instance.Bootstrap();
                        AuthorizationManager.Instance.Initialize();
                        AuthorizationManager.Instance.Start();
                        if (AuthorizationManager.Instance.State != RunState.Running)
                        {
                            Log(method, LogLevel.Warn, "Failed Starting AuthorizationManager");
                            return false;
                        }

                        Log(method, LogLevel.Info, "Starting IdentityManager");
                        IdentityManager.Instance.Bootstrap();
                        IdentityManager.Instance.Initialize();
                        IdentityManager.Instance.Start();
                        if (IdentityManager.Instance.State != RunState.Running)
                        {
                            Log(method, LogLevel.Warn, "Failed Starting IdentityManager");
                            return false;
                        }

                        Log(method, LogLevel.Info, "Starting AuthenticationManager");
                        AuthenticationManager.Instance.Bootstrap();
                        AuthenticationManager.Instance.Initialize();
                        AuthenticationManager.Instance.Start();
                        if (AuthenticationManager.Instance.State != RunState.Running)
                        {
                            Log(method, LogLevel.Warn, "Failed Starting AuthenticationManager");
                            return false;
                        }

                        Log(method, LogLevel.Info, "Starting SessionManager");
                        SessionManager.Instance.Bootstrap();
                        SessionManager.Instance.Initialize();
                        SessionManager.Instance.Start();
                        if (SessionManager.Instance.State != RunState.Running)
                        {
                            Log(method, LogLevel.Warn, "Failed Starting SessionManager");
                            return false;
                        }

                        return true;
                    }
                    catch(Exception e)
                    {
                        Log(method, LogLevel.Warn, "Encountered a fault: "+e.Message);
                    }
                }
            }
            return false;
        }

        private static LogProviderBase logger;
        private static void Log(string method, LogLevel level, string message)
        {
            if (logger == null)
                logger = LogManager.Instance.GetProvider(typeof(OncorServer));

            if (logger != null)
                logger.Log(method, LogLevel.Info, message);
        }
    }
}
