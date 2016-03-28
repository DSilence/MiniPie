﻿using System.Linq;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;
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
            using (var mgr = new Squirrel.UpdateManager(string.Empty))
            {
                // Note, in most of these scenarios, the app exits after this method
                // completes!
                SquirrelAwareApp.HandleEvents(
                  onInitialInstall: v =>
                  {
                      mgr.CreateShortcutForThisExe();
                      Application.Current.Shutdown();
                  },
                  onAppUpdate: v =>
                  {
                      mgr.CreateShortcutForThisExe();
                      Application.Current.Shutdown();
                  },
                  onAppUninstall: v =>
                  {
                      mgr.RemoveShortcutForThisExe();
                      Application.Current.Shutdown();
                  },
                  onFirstRun: () => {});
            }
        }
    }
}
