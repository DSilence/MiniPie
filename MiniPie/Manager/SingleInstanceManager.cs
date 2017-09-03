using System;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.ApplicationServices;
using MiniPie.Core;
using Squirrel;
using StartupEventArgs = Microsoft.VisualBasic.ApplicationServices.StartupEventArgs;

namespace MiniPie.Manager
{
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        private App _application;
        private System.Collections.ObjectModel.ReadOnlyCollection<string> _commandLine;

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            ProcessSquirrelStartup();
            // First time _application is launched
            _commandLine = eventArgs.CommandLine;
            _application = new App();
            _application.Run();
            return false;
        }
       

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            ProcessSquirrelStartup();
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            _commandLine = eventArgs.CommandLine;
            string code = _commandLine.FirstOrDefault();
            if (code != null && !code.StartsWith("-"))
            {
                _application?.Bootstrapper?.ProcessTokenUpdate(code);
            }
        }

        private void ProcessSquirrelStartup()
        {
#if RELEASE
            using (var mgr = new Squirrel.UpdateManager(string.Empty))
            {
                // Note, in most of these scenarios, the app exits after this method
                // completes!
                SquirrelAwareApp.HandleEvents(
                  onInitialInstall: v =>
                  {
                      mgr.CreateShortcutForThisExe();
                      UriProtocolManager.RegisterUrlProtocol();
                      Environment.Exit(0);
                  },
                  onAppUpdate: v =>
                  {
                      mgr.CreateShortcutForThisExe();
                      Environment.Exit(0);
                  },
                  onAppUninstall: v =>
                  {
                      mgr.RemoveShortcutForThisExe();
                      UriProtocolManager.UnregisterUrlProtocol();
                      AppContracts contracts = new AppContracts();
                      var settingsPersistor =
                          new JsonPersister<AppSettings>(Path.Combine(contracts.SettingsLocation,
                              contracts.SettingsFilename));
                      string dir = Path.Combine(string.IsNullOrEmpty(settingsPersistor.Instance.CacheFolder)
                          ? contracts.SettingsLocation
                          : settingsPersistor.Instance.CacheFolder, CoverService.CacheDirName);
                      var log = new ProductionLogger();
                      log.Fatal("Uninstalling Minipie. Removing CoverCache from ->" + dir);
                      try
                      {
                          if (Directory.Exists(dir))
                          {
                              Directory.Delete(dir, true);
                          }
                      }
                      catch (Exception e)
                      {
                          log.FatalException(e.Message, e);
                      }
                      Environment.Exit(0);
                  },
                  onFirstRun: () =>
                  {
                  });
            }
#endif
        }
    }
}
