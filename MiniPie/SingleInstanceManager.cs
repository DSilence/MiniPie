using System.Linq;
using Microsoft.VisualBasic.ApplicationServices;

namespace MiniPie
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
            // First time _application is launched
            _commandLine = eventArgs.CommandLine;
            _application = new App();
            _application.Run();
            return true;
        }
       

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            _commandLine = eventArgs.CommandLine;
            string code = _commandLine.FirstOrDefault();
            if (code != null)
            {
                _application?.Bootstrapper?.ProcessTokenUpdate(code);
            }
        }
    }
}
