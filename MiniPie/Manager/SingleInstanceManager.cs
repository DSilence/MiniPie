using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;
using NuGet;
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
            ProcessAdminArguments(eventArgs.CommandLine);
            ProcessSquirrelStartup();
            // First time _application is launched
            _commandLine = eventArgs.CommandLine;
            _application = new App();
            _application.Run();
            return false;
        }
       

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            ProcessAdminArguments(eventArgs.CommandLine);
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

        private Process RunAsAdmin(string arguments)
        {
            var process = Process.GetCurrentProcess();
            var fileName = process.MainModule.FileName;
            var startInfo = new ProcessStartInfo(fileName)
            {
                Arguments = arguments,
                Verb = "runas"
            };
            return new Process
            {
                StartInfo = startInfo
            };
        }

        private void ProcessAdminArguments(IList<string> arguments)
        {
            if (arguments == null || arguments.IsEmpty())
            {
                return;
            }
            if (arguments.Contains("-register"))
            {
                UriProtocolManager.RegisterUrlProtocol();
                Environment.Exit(0);
            }
            if (arguments.Contains("-unregister"))
            {
                UriProtocolManager.UnregisterUrlProtocol();
                Environment.Exit(0);
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
                      var register = RunAsAdmin("-register");
                      register.Exited += (sender, args) =>
                      {
                          mgr.CreateShortcutForThisExe();
                          Environment.Exit(0);
                      };
                      register.Start();
                  },
                  onAppUpdate: v =>
                  {
                      mgr.CreateShortcutForThisExe();
                      Environment.Exit(0);
                  },
                  onAppUninstall: v =>
                  {
                      var register = RunAsAdmin("-unregister");
                      register.Exited += (sender, args) =>
                      {
                          mgr.RemoveShortcutForThisExe();
                          Environment.Exit(0);
                      };
                      register.Start();
                  },
                  onFirstRun: () =>
                  {
                  });
            }
        }
    }
}
